using LudeonTK;
using RimWorld;
using Verse;

namespace TR.Components;
/* Tiberium Map Component
 * Description:
 * In this component all the major Tiberium-related mechanics get managed
 * Tiberium Information - Main Info on tiberium positions, and cell-states
 *
 */

public class MapComponent_Tiberium : MapComponentWithDraw
{
    [TweakValue("MapComponent_TibDrawBool")]
    public static bool DrawBool = false;

    [TweakValue("MapComponent_TibHediffBool")]
    public static bool HediffBool = false;


    [TweakValue("MapComponent_TibHarvesterBool")]
    public static bool HarvesterBool = false;
    
    public HashSet<IntVec3> AffectedCells = new();
    public IntVec3 currentDebugCell;

    public TiberiumFloraMapInfo FloraInfo; // Tiberium Plant life, Gardens, Environment
    public HarvesterMapInfo HarvesterInfo;
    public MapPawnInfo MapPawnInfo; // Currently infected pawns, animals, colonists, visitors, etc
    public TiberiumPollutionMapInfo PollutionInfo;
    public TiberiumStructureInfo StructureInfo;

    // Artificial
    public SuppressionMapInfo SuppressionInfo;
    public TiberiumTerrainInfo TerrainInfo;

    //Active Components
    public Info.TiberiumAffecter TiberiumAffecter;

    //Map Information - This encloses all the different areas of a map which can be affected by tiberium, and ensures correct and dynamic effects
    // Natural
    public TiberiumMapInfo TiberiumInfo; // Tiberium Crystals, Pods, etc, all variations
    public Info.TiberiumSpreader TiberiumSpreader;
    
    //Debug
    public Region currentDebugRegion;
    private bool dirtyIterator;
    public TiberiumInfectionInfo InfectionInfo;
    public HashSet<IntVec3> IteratorTiles = new();
    public TiberiumStructureInfo StructureInfo;

    private int TiberiumArrivalTick;
    public TiberiumMapInfo TiberiumInfo;

    //Affected Objects Iterator
    private IEnumerator<IntVec3> TileIterator;

    public MapComponent_Tiberium(Map map) : base(map)
    {
        TiberiumInfo = new TiberiumMapInfo(map);
        StructureInfo = new TiberiumStructureInfo(map);
        InfectionInfo = new TiberiumInfectionInfo(map);
        
        FloraInfo = new TiberiumFloraMapInfo(map);
        TerrainInfo = new TiberiumTerrainInfo(map);
        PollutionInfo = new TiberiumPollutionMapInfo(map);
        MapPawnInfo = new MapPawnInfo(map);

        SuppressionInfo = new SuppressionMapInfo(map);
        HarvesterInfo = new HarvesterMapInfo(map);

        TiberiumAffecter = new Info.TiberiumAffecter(map);
        TiberiumSpreader = new Info.TiberiumSpreader(map);
    }

    public MapComponent_Suppression Suppression => map.GetComponent<MapComponent_Suppression>();
    public MapComponent_TNWManager TNWManager => map.GetComponent<MapComponent_TNWManager>();

    public bool TiberiumAvailable => TiberiumInfo.TiberiumCrystals[HarvestType.Valuable].Count >
                                     TNWManager.ReservationManager.ReservedTypes[HarvestType.Valuable];

    public bool MossAvailable => TiberiumInfo.TiberiumCrystals[HarvestType.Unvaluable].Count >
                                 TNWManager.ReservationManager.ReservedTypes[HarvestType.Unvaluable];

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        if (!FloraInfo.HasBeenInitialized)
            FloraInfo.InfoInit();
        if (!TerrainInfo.HasBeenInitialized)
            TerrainInfo.InfoInit();
    }

    public void TiberiumMapInterfaceUpdate()
    {
        TiberiumAffecter.HediffGrid.Update();
    }

    public override void MapGenerated()
    {
        //Runs once on map generation
        base.MapGenerated();
        FloraInfo.InfoInit();
        TerrainInfo.InfoInit();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref TiberiumArrivalTick, "arrivalTick");
        Scribe_Deep.Look(ref TiberiumInfo, "tiberiumMapInfo", map);
        Scribe_Deep.Look(ref FloraInfo, "FloraInfo", map);
        Scribe_Deep.Look(ref TiberiumAffecter, "affecter", map);
        base.ExposeData();
    }

    /*
    [TweakValue("MapComponent_QuadTreePoints", 0, 100)]
    public static int QuadTreePoints = 1;

    [TweakValue("MapComponent_QuadDrawBool", 0f, 100f)]
    public static bool QuadDrawReset = false;

    //TEST QuadTree
    public QuadTree TestTree;
    */

    public override void MapComponentUpdate()
    {
        base.MapComponentUpdate();
        if (DrawBool)
        {
            TiberiumInfo.TiberiumGrid.drawer.RegenerateMesh();
            TiberiumInfo.TiberiumGrid.drawer.MarkForDraw();
            TiberiumInfo.TiberiumGrid.drawer.CellBoolDrawerUpdate();

            TiberiumInfo.FloraGrid.drawer.RegenerateMesh();
            TiberiumInfo.FloraGrid.drawer.MarkForDraw();
            TiberiumInfo.FloraGrid.drawer.CellBoolDrawerUpdate();
            //Suppression.SuppressionGrid.drawer.RegenerateMesh();
            //Suppression.SuppressionGrid.drawer.MarkForDraw();
            //Suppression.SuppressionGrid.drawer.CellBoolDrawerUpdate();
        }
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        IterateThroughTiles();
        if (Find.TickManager.TicksGame % 250 == 0) TiberiumInfo.TiberiumGrid.UpdateDirties();
    }

    public override void MapComponentDraw()
    {
    }

    public IEnumerable<Thing> TiberiumSetForHarvester(TR.Harvester harvester)
    {
        Log.Message("Getting tiberium set for " + harvester + " with mode " + harvester.harvestMode);
        Log.Message("Count for that mode: " + TiberiumInfo.TiberiumCrystals[HarvestType.Valuable]?.Count);
        var things = new List<TiberiumCrystal>();
        switch (harvester.harvestMode)
        {
            case TR.HarvestMode.Nearest:
                things = TiberiumInfo.TiberiumCrystals[HarvestType.Valuable]; break;
            case TR.HarvestMode.Value:
                things = TiberiumInfo.TiberiumCrystals[HarvestType.Valuable]
                    .Where(t => t.def == TiberiumInfo.MostValuableType).ToList(); break;
            case TR.HarvestMode.Moss:
                things = TiberiumInfo.TiberiumCrystals[HarvestType.Unvaluable]; break;
        }

        Log.Message("Revisited count on things list " + things.Count());
        return things.Select(t => t as Thing);
        ;
    }

    public void IterateThroughTiles()
    {
        if (!IteratorTiles.Any())
            return;
        //Setup Iterator
        if (TileIterator == null || dirtyIterator)
        {
            TileIterator = IteratorTiles.InRandomOrder().GetEnumerator();
            dirtyIterator = false;
        }

        //Affect Objects
        if (TileIterator?.Current.IsValid ?? false)
        {
            currentDebugCell = TileIterator.Current;
            TiberiumCrystal affecter =
                currentDebugCell.CellsAdjacent8Way().Select(c => c.GetTiberium(map)).FirstOrDefault();
            AffectPotentialObject(currentDebugCell, affecter);
        }

        if (!TileIterator.MoveNext())
            dirtyIterator = true;
    }

    private void AffectPotentialObject(IntVec3 cell, TiberiumCrystal affecter)
    {
        if (affecter == null) return;
        ThingDef newThing = null;
        float damageFactor = 1;
        var haulable = cell.GetFirstHaulable(map);
        if (haulable != null && affecter.def.tiberium.entityDamage.Average > 0 &&
            haulable.CanBeDamagedByTib(out damageFactor))
        {
            if (haulable.def.IsNutritionGivingIngestible)
                damageFactor += 0.33f;
            if (haulable.IsCorruptableChunk())
                newThing = affecter.def.chunk;
            haulable.TakeDamage(new DamageInfo(DamageDefOf.Deterioration,
                damageFactor * TRUtils.Range(affecter.def.tiberium.entityDamage)));
            if (newThing != null && TRUtils.Chance(MainTCD.Main.ChunkCorruptionChance))
            {
                GenSpawn.Spawn(newThing, haulable.Position, map);
                if (!haulable.DestroyedOrNull())
                    haulable.DeSpawn();
            }

            return;
        }

        var building = cell.GetFirstBuilding(map);
        if (building != null && affecter.def.tiberium.buildingDamage.Average > 0 &&
            building.CanBeDamagedByTib(out damageFactor))
        {
            var chance = 1f;
            if (building is Building_SteamGeyser)
            {
                newThing = TiberiumDefOf.TiberiumGeyser;
                chance *= MainTCD.Main.GeyserCorruptionChance;
            }

            if (building.def.mineable)
            {
                newThing = affecter.def.rock;
                chance *= MainTCD.Main.RockCorruptionChance;
            }

            if (building.def.IsWall())
            {
                newThing = affecter.def.wall;
                chance *= MainTCD.Main.WallCorruptionChance;
            }

            building.TakeDamage(new DamageInfo(DamageDefOf.Deterioration,
                damageFactor * TRUtils.Range(affecter.def.tiberium.buildingDamage)));
            if (newThing != null && TRUtils.Chance(chance))
            {
                GenSpawn.Spawn(newThing, building.Position, map);
                if (!building.DestroyedOrNull())
                    building.DeSpawn();
            }
        }
    }

    public void AddTiberiumPlant(TiberiumPlant plant, bool respawn)
    {
        FloraInfo.RegisterTiberiumPlant(plant);
    }

    public void RemoveTiberiumPlant(TiberiumPlant plant)
    {
        FloraInfo.DeregisterTiberiumPlant(plant);
    }

    public void AddTiberium(TiberiumCrystal crystal, bool respawn)
    {
        TiberiumInfo.RegisterTiberium(crystal);
        AddCells(crystal);
    }

    public void RemoveTiberium(TiberiumCrystal crystal)
    {
        TiberiumInfo.DeregisterTiberium(crystal);
        RemoveCells(crystal);
    }

    private void AddCells(TiberiumCrystal crystal)
    {
        var cells = crystal.CellsAdjacent8WayAndInside().ToList();
        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            if (!cell.InBounds(map)) continue;
            if (cell.GetTiberium(map) == null)
                IteratorTiles.Add(cell);
            else
                IteratorTiles.Remove(cell);

            if (AffectedCells.Contains(cell)) continue;
            AffectedCells.Add(cell);
            dirtyIterator = true;
        }
    }

    private void RemoveCells(TiberiumCrystal crystal)
    {
        var cells = crystal.CellsAdjacent8WayAndInside();
        IteratorTiles.Add(crystal.Position);
        for (var i = 0; i < cells.Count(); i++)
        {
            var cell = cells.ElementAt(i);
            if (!cell.InBounds(map)) continue;
            var rect = new CellRect(cell.x - 1, cell.z - 1, 3, 3);
            var flag = true;
            for (var ii = 0; ii < rect.Cells.Count(); ii++)
            {
                var cell2 = rect.Cells.ElementAt(ii);
                TiberiumCrystal crystal2 = cell2.GetTiberium(map);
                if (crystal2 != null && crystal2 != crystal)
                    flag = false;
            }

            if (!flag) continue;
            AffectedCells.Remove(cell);
            IteratorTiles.Remove(cell);
            dirtyIterator = true;
        }
    }
}