using LudeonTK;
using TR.Info;
using TR.Interfaces;
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

    public MapComponent_Tiberium(Map map) : base(map)
    {
        TiberiumInfo = new TiberiumMapInfo(map);
        FloraInfo = new TiberiumFloraMapInfo(map);
        TerrainInfo = new TiberiumTerrainInfo(map);
        StructureInfo = new TiberiumStructureInfo(map);
        PollutionInfo = new TiberiumPollutionMapInfo(map);
        MapPawnInfo = new MapPawnInfo(map);

        SuppressionInfo = new SuppressionMapInfo(map);
        HarvesterInfo = new HarvesterMapInfo(map);

        TiberiumAffecter = new Info.TiberiumAffecter(map);
        TiberiumSpreader = new Info.TiberiumSpreader(map);
    }


    public bool TiberiumAvailable => TiberiumInfo.TiberiumCrystals[HarvestType.Valuable].Count > 0;

    public bool MossAvailable => TiberiumInfo.TiberiumCrystals[HarvestType.Unvaluable].Count > 0;

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
        base.ExposeData();
        Scribe_Deep.Look(ref TiberiumInfo, "tiberiumMapInfo", map);
        Scribe_Deep.Look(ref FloraInfo, "FloraInfo", map);
        Scribe_Deep.Look(ref TiberiumAffecter, "affecter", map);
    }


    public override void MapComponentDraw()
    {
        base.MapComponentDraw();
    }

    public override void MapComponentOnGUI()
    {
        base.MapComponentOnGUI();
        PollutionInfo.UpdateOnGUI();
        if (HediffBool)
            TiberiumAffecter.HediffGrid.DrawValues();
        if (HarvesterBool)
        {
        }
    }

    public override void MapComponentUpdate()
    {
        base.MapComponentUpdate();
        //PollutionInfo.Update();
        PollutionInfo.Draw();
        if (DrawBool)
        {
            TiberiumInfo.Draw();
            FloraInfo.Draw();
            TerrainInfo.Draw();

            HarvesterInfo.Draw();
            StructureInfo.Draw();
            //Suppression.SuppressionGrid.drawer.RegenerateMesh();
            //Suppression.SuppressionGrid.drawer.MarkForDraw();
            //Suppression.SuppressionGrid.drawer.CellBoolDrawerUpdate();
        }
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        TiberiumInfo.Tick();
        PollutionInfo.Tick();
        SuppressionInfo.Tick();
        TiberiumAffecter.Tick();
        TiberiumSpreader.Tick();
    }

    public void RegisterNewThing(Thing thing)
    {
        if (thing is IPollutionSource source) PollutionInfo.RegisterSource(source);
    }

    public void DeregisterThing(Thing thing)
    {
        if (thing is IPollutionSource source) PollutionInfo.DeregisterSource(source);
    }

    public void RegisterTiberiumBuilding(TRBuilding building)
    {
        StructureInfo.TryRegister(building);
        if (building is TiberiumProducer p)
            TiberiumSpreader.RegisterField(p);
    }

    public void DeregisterTiberiumBuilding(TRBuilding building)
    {
        StructureInfo.Deregister(building);
        if (building is TiberiumProducer p)
            TiberiumSpreader.DeregisterField(p);
    }

    public void RegisterTiberiumCrystal(TiberiumCrystal crystal)
    {
        TiberiumInfo.RegisterTiberium(crystal);
        TerrainInfo.Notify_TibSpawned(crystal);
        TiberiumAffecter.Notify_TibChanged();
        //AddCells(crystal);
    }

    public void DeregisterTiberiumCrystal(TiberiumCrystal crystal)
    {
        TiberiumInfo.DeregisterTiberium(crystal);
        TiberiumAffecter.Notify_TibChanged();
        //RemoveCells(crystal);
    }

    public void RegisterTiberiumPlant(TiberiumPlant plant)
    {
        FloraInfo.RegisterTiberiumPlant(plant);
    }

    public void DeregisterTiberiumPlant(TiberiumPlant plant)
    {
        FloraInfo.DeregisterTiberiumPlant(plant);
    }

    /*
    public void AddTiberiumPlant(TiberiumPlant plant)
    {
        FloraInfo.RegisterTiberiumPlant(plant);
    }

    public void RemoveTiberiumPlant(TiberiumPlant plant)
    {
        FloraInfo.DeregisterTiberiumPlant(plant);
    }

    public void AddTiberium(TiberiumCrystal crystal)
    {
        TiberiumInfo.RegisterTiberium(crystal);
        AddCells(crystal);
    }

    public void RemoveTiberium(TiberiumCrystal crystal)
    {
        TiberiumInfo.DeregisterTiberium(crystal);
        RemoveCells(crystal);
    }
    */
    /*
    private void AddCells(TiberiumCrystal crystal)
    {
        List<IntVec3> cells = crystal.CellsAdjacent8WayAndInside().ToList();
        for (int i = 0; i < cells.Count; i++)
        {
            IntVec3 cell = cells[i];
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
        IEnumerable<IntVec3> cells = crystal.CellsAdjacent8WayAndInside();
        IteratorTiles.Add(crystal.Position);
        for (int i = 0; i < cells.Count(); i++)
        {
            IntVec3 cell = cells.ElementAt(i);
            if (!cell.InBounds(map)) continue;
            CellRect rect = new CellRect(cell.x - 1, cell.z - 1, 3, 3);
            bool flag = true;
            for (int ii = 0; ii < rect.Cells.Count(); ii++)
            {
                IntVec3 cell2 = rect.Cells.ElementAt(ii);
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
    */
}