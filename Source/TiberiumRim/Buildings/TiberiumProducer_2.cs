using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TiberiumRim.Tiberium;
using TR;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class TiberiumProducer : TRBuilding
{
    private readonly List<TiberiumCrystal> boundCrystals = new();
    private readonly IEnumerator<TiberiumCrystal> growingCrystals;

    public new TiberiumProducerDef def;

    //DebugSettings
    public bool fastGrow = false;
    private bool showAffect = false;
    private bool showField = false;
    private bool showPotentialField = false;
    private bool showTileIterator = false;
    public bool turnOffLight = false;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        def = (TiberiumProducerDef)base.def;
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
    }

    public override void Tick()
    {
        if (fastGrow)
            TiberiumForceGrow();
    }

    //Tiberium Debug Growth Iterator
    private void TiberiumForceGrow()
    {
        var curCrystal = growingCrystals?.Current;
        curCrystal?.TiberiumTick();

        if (!growingCrystals?.MoveNext() ?? false)
            growingCrystals.Reset();
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override void Print(SectionLayer layer)
    {
        base.Print(layer);
    }
}

public class TiberiumProducer3 : TRBuilding
{
    private readonly List<CellPath> cellPaths = new();
    private readonly bool isEvolved = false;
    public List<TiberiumCrystal> boundCrystals = new();
    private float curRadius;
    public new TiberiumProducerDef def;
    private TiberiumProducerDef evolvesTo;

    public bool fastGrow;

    //TODO: Replace cell lists with areas 
    //TODO: Make custom area class
    private HashSet<IntVec3> fieldCellsList = new();
    private float floodRadius;
    public List<TiberiumCrystal> growingCrystals = new();

    //Maturing
    public List<IntVec3> InitialCells = new();


    private string inspectString = "";
    private bool isGroundZero;
    public bool NoGrowth;
    public bool NoReprint;

    //Debug
    public bool NoSpread;
    public bool NoTerrain;
    private List<IntVec3> pathCells = new();

    //Ground Zero
    public Building researchCrane;
    private bool showAffect;
    private bool showField;
    private bool showPotentialField;
    private bool showTileIterator;
    public bool stopGrowth;
    public bool stopTicking = false;

    private int tickLeft = 750;
    private int ticksToEvolution;
    private int ticksToMature;

    //Ticker
    private int ticksToSpawn = 100;
    private int ticksToSpore = 100;
    public bool turnOffLight;

    public override string Label
    {
        get
        {
            if (isGroundZero)
                return base.Label + " " + "(GZ)";
            return base.Label;
        }
    }

    public TiberiumTerrainDef Terrain => def.tiberiumTerrain.RandomElement();

    public MapComponent_Tiberium Manager => Map.GetComponent<MapComponent_Tiberium>();

    public TiberiumCrystalDef TiberiumCrystal
    {
        get
        {
            if (def.tiberiumTypes.NullOrEmpty())
                return null;
            return def.tiberiumTypes.RandomElement();
        }
    }

    public HashSet<IntVec3> FieldCells => fieldCellsList;

    public float GrowRadius => isGroundZero ? def.spawner.growRadius * 2.5f : def.spawner.growRadius;

    public float WokePercent => 1f - ticksToMature / (def.daysToMature * GenDate.TicksPerDay);

    public bool ShouldSpawnSpore => isGroundZero && ticksToSpore <= 0 && MatureEnough;
    public bool ShouldSpawn => def.tiberiumTypes.Any() && ticksToSpawn <= 0 && MatureEnough;
    public bool ShouldEvolve => evolvesTo != null && ticksToEvolution <= 0;
    private bool MatureEnough => IsMature || ticksToMature < def.spawner.minDaysToSpread * GenDate.TicksPerDay;
    public bool IsMature => ticksToMature <= 0 && !InitialCells.Any();

    public bool ResearchBound
    {
        get
        {
            if (researchCrane == null)
                researchCrane = (Building)Map.thingGrid.ThingAt(Position, TiberiumDefOf.TiberiumResearchCrane);
            return !researchCrane.DestroyedOrNull();
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksToSpawn, "ticksToSpawn");
        Scribe_Values.Look(ref ticksToSpore, "ticksToSpore");
        Scribe_Values.Look(ref ticksToMature, "ticksToMature");
        Scribe_Values.Look(ref ticksToEvolution, "ticksToEvolution");
        Scribe_Values.Look(ref isGroundZero, "isGroundZero");
        Scribe_Values.Look(ref floodRadius, "floodRadius");
        Scribe_Collections.Look(ref InitialCells, "InitCells");
        Scribe_Collections.Look(ref fieldCellsList, "fieldCells");
        Scribe_Defs.Look(ref evolvesTo, "evolvesTo");
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        //Basic set-up of non-saved components
        def = base.def as TiberiumProducerDef;

        if (respawningAfterLoad)
            //We USED TO guess a random amount of field cells, if respawning
            //SetPotentialFieldCells();
            return;
        // First Time Spawn //
        //Setting up values
        ResetSpawnTicks();
        if (def.spore != null)
            ResetSporeCounter();

        //If possible, setup potential evolution
        SetEvolution();

        //If this is the first producer, make it Ground Zero
        WorldTiberiumComp.SetupGroundZero(this, Map, ref isGroundZero);

        if (isEvolved)
            return;

        ticksToMature = (int)(GenDate.TicksPerDay * def.daysToMature);
        //Setting up initial cells, may take a while thus making it a long event
        SetInitialCells();
        //LongEventHandler.QueueLongEvent(SetInitialCells, "SettingInitialProducerCells", false, null);

        var AdjacentCells = GenAdj.CellsAdjacent8Way(this).ToList();

        bool EndCond(IntVec3 c)
        {
            return IsMature && Position.DistanceTo(c) >= floodRadius;
        }

        for (var i = 0; i < AdjacentCells.Count - 1; i++)
            if (i % 3 == 0)
                cellPaths.Add(new CellPath(map, AdjacentCells[i], IntVec3.Invalid, Position, floodRadius, EndCond));

        foreach (var adjCell in AdjacentCells.Where(c => c.InBounds(map)))
            if (!def.tiberiumTerrain.NullOrEmpty())
            {
                var terrain = adjCell.GetTerrain(Map);
                var terr = def.tiberiumTerrain.Find(t => t.TerrainSupportFor(terrain) != null);
                if (terr == null) continue;
                var newTerr = GenTiberium.TerrainFrom(terrain, terr);
                Map.terrainGrid.SetTerrain(adjCell, newTerr);
            }
            else
            {
                GenTiberium.SetTiberiumTerrain(adjCell, Map, TiberiumCrystal);
            }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        DeSpawn(mode, false);
    }

    /* Initial Cell Set-Up - Currently Suspended
    private void SetPotentialFieldCells()
    {
        if (GrowRadius <= 0) return;
        FloodFiller floodFill = new FloodFiller(Map);
        bool Predicate(IntVec3 x) => x.GetTerrain(Map) is TiberiumTerrainDef && x.DistanceTo(Position) <= GrowRadius;
        void Action(IntVec3 c)
        {
            FieldCells.Add(c);
        }
        var potentialCellCount = lastFieldCellCount > 0 ? lastFieldCellCount : int.MaxValue;
        floodFill.FloodFill(Position, Predicate, Action, potentialCellCount);
    }
    */

    private void SetInitialCells()
    {
        var radialCellCount = GenRadial.NumCellsInRadius(GrowRadius);

        bool Predicate(IntVec3 c)
        {
            return c.SupportsTiberiumTerrain(Map);
        }

        void Action(IntVec3 c)
        {
            InitialCells.Add(c);
            var curDist = c.DistanceTo(Position);
            if (curDist > floodRadius)
                floodRadius = curDist;
        }

        var flood = new TiberiumFloodInfo(Map, Predicate, Action);
        flood.TryMakeFlood(out var cells, this.OccupiedRect(), radialCellCount);
    }

    public void DeSpawn(DestroyMode mode, bool replace)
    {
        if (!replace)
        {
            var killVer = def.killedVersion;
            if (killVer != null)
                GenSpawn.Spawn(killVer, Position, Map);

            if (def.leaveTiberium)
            {
                var amt = 6;
                for (var i = 0; i < amt; i++)
                {
                    var crystal = ThingMaker.MakeThing(TiberiumCrystal) as TiberiumCrystal;
                    Predicate<IntVec3> pred = c => c.GetTiberium(Map) == null;
                    GenPlace.TryPlaceThing(crystal, Position, Map, ThingPlaceMode.Near, null, pred);
                }
            }
        }

        base.DeSpawn();
    }

    public override void Tick()
    {
        base.Tick();
        if (!Spawned)
            return;

        if (fastGrow)
            for (var i = growingCrystals.Count - 1; i >= 0; i--)
                growingCrystals[i].TiberiumTick();

        if (tickLeft <= 0)
        {
            if (!IsMature)
            {
                SpreadTerrain();
                GrowCellPaths();
            }

            if (ShouldSpawnSpore)
            {
                SpawnBlossomSpore();
                ResetSporeCounter();
            }

            if (ShouldSpawn)
            {
                SpawnTiberium();
                ResetSpawnTicks();
            }

            if (ShouldEvolve)
                SpawnEvolved(evolvesTo);

            tickLeft = 750;
        }

        tickLeft--;

        if (ticksToMature > 0)
        {
            ticksToMature--;
            return;
        }

        if (ticksToSpore > 0)
            ticksToSpore--;

        if (ticksToSpawn > 0)
            ticksToSpawn--;
    }

    public override void TickRare()
    {
        base.TickRare();
        if (!Spawned)
            return;

        if (fastGrow)
            for (var j = 0; j < 150; j++)
            for (var i = growingCrystals.Count - 1; i >= 0; i--)
                growingCrystals[i].TiberiumTick();

        if (!IsMature)
        {
            SpreadTerrain();
            GrowCellPaths();
        }

        if (ShouldSpawnSpore)
        {
            SpawnBlossomSpore();
            ResetSporeCounter();
        }

        if (ShouldSpawn)
        {
            SpawnTiberium();
            ResetSpawnTicks();
        }

        if (ShouldEvolve)
            SpawnEvolved(evolvesTo);

        if (ticksToMature > 0)
        {
            ticksToMature -= GenTicks.TickRareInterval;
            return;
        }

        if (ticksToSpore > 0)
            ticksToSpore -= GenTicks.TickRareInterval;
        if (ticksToSpawn > 0)
            ticksToSpawn -= GenTicks.TickRareInterval;
    }

    private void SpreadTerrain()
    {
        var radius = Mathf.Lerp(0f, floodRadius, WokePercent);
        curRadius = radius;
        float radialCount = GenRadial.NumCellsInRadius(radius);
        //var cells = InitialCells.Where(c => c.DistanceTo(Position) <= radius);
        for (var i = 0; i < radialCount; i++)
        {
            var cell = Position + GenRadial.RadialPattern[i];
            if (!InitialCells.Contains(cell)) continue;

            InitialCells.Remove(cell);
            var terrain = cell.GetTerrain(Map);

            /*
            if (FieldCells.Contains(cell))
            {
                Log.Message(this + " Contains existing cell");
            }
            */

            AddFieldCell(cell);
            //lastFieldCellCount++;
            if (terrain.IsTiberiumTerrain()) continue;

            TiberiumTerrainDef newTerr = null;
            if (!def.tiberiumTerrain.NullOrEmpty())
            {
                newTerr = GenTiberium.TerrainFrom(terrain, Terrain);
                if (newTerr != null)
                    Map.terrainGrid.SetTerrain(cell, newTerr);
            }

            if (newTerr == null && !def.customTerrain.NullOrEmpty())
                newTerr = def.customTerrain.Find(s => s.TerrainTag.SupportsDef(terrain)).TerrainOutcome;
            if (newTerr == null)
                newTerr = GenTiberium.SetTiberiumTerrain(cell, Map, TiberiumCrystal);

            if (newTerr != null && def.growsFlora && cell.Standable(Map) && cell.GetFirstBuilding(Map) == null)
                TrySpreadFlora(cell, newTerr);
        }
    }

    private void GrowCellPaths()
    {
        foreach (var path in cellPaths) path.Grow(curRadius, ref pathCells);
    }

    public void TrySpreadFlora(IntVec3 pos, TiberiumTerrainDef terrain)
    {
        if (pos.GetPlant(Map) is TiberiumPlant) return;
        var distance = Position.DistanceTo(pos);
        var chance = 1f - Mathf.InverseLerp(0f, floodRadius, distance);
        ;
        if (TRUtils.Chance(chance * terrain.plantChanceFactor))
        {
            var flora = SelectedFloraAt(distance, terrain);
            if (flora != null)
            {
                var plant = ThingMaker.MakeThing(flora);
                if (plant is Plant p)
                    p.Growth = TRUtils.Range(0.1f, 0.55f);
                GenSpawn.Spawn(plant, pos, Map);
            }
        }
    }

    private ThingDef SelectedFloraAt(float distance, TiberiumTerrainDef terrain)
    {
        return def.SelectPlantByDistance(distance, floodRadius, terrain);
    }

    public void AddFieldCell(IntVec3 cell)
    {
        fieldCellsList.Add(cell);
        foreach (var def in def.tiberiumTypes)
            TiberiumComp.TiberiumInfo.TiberiumGrid.SetFieldColor(cell, true, def.TiberiumValueType);
    }

    public void RemoveFieldCell(IntVec3 cell)
    {
        fieldCellsList.Remove(cell);
        foreach (var def in def.tiberiumTypes)
            TiberiumComp.TiberiumInfo.TiberiumGrid.SetFieldColor(cell, false, def.TiberiumValueType);
    }

    private void SpawnBlossomSpore()
    {
        var dest = TiberiumComp.StructureInfo.GetBlossomDestination();
        if (!dest.IsValid) return;
        var spore = GenTiberium.SpawnBlossomSpore(Position, dest, Map, def.spore.Blossom(), this);
        LetterMaker.MakeLetter("Blossom Spore", "A blossom spore has appeared, and will fly to this position.",
            LetterDefOf.NeutralEvent, new LookTargets(spore.endCell, Map));
    }

    private void SpawnTiberium()
    {
        int spores;
        List<IntVec3> cells;
        switch (def.spawner.spawnMode)
        {
            case TiberiumSpawnMode.Direct:
                cells = this.CellsAdjacent8WayAndInside().Where(c =>
                    c.InBounds(Map) && c.GetTiberium(Map) == null && c.GetFirstBuilding(Map) == null).ToList();
                if (cells.Any())
                    TrySpawnTiberiumAt(cells.RandomElement());
                break;
            case TiberiumSpawnMode.Spore:
                cells = FieldCells.Where(c =>
                    c.InBounds(Map) && c.GetTiberium(Map) == null && c.GetFirstBuilding(Map) == null &&
                    c.GetPlant(Map) == null &&
                    !c.Roofed(Map)).ToList();
                if (cells.Any())
                    GenTiberium.SpawnSpore(this.OccupiedRect(), cells.RandomElement(), Map, TiberiumCrystal, this);
                break;
            case TiberiumSpawnMode.SporeBurst:
                spores = TRUtils.Range(def.spawner.explosionRange);
                GenTiberium.SpawnSpore(this.OccupiedRect(), def.spawner.sporeExplosionRadius, Map, TiberiumCrystal,
                    this,
                    spores, true);
                break;
            case TiberiumSpawnMode.SporeExplosion:
                spores = TRUtils.Range(def.spawner.explosionRange);
                GenTiberium.SpawnSpore(this.OccupiedRect(), def.spawner.sporeExplosionRadius, Map, TiberiumCrystal,
                    this,
                    spores, true);
                GenExplosion.DoExplosion(Position, Map, 6.76f, DamageDefOf.Bomb, this);
                break;
        }
    }

    public bool InsideGrowPath(IntVec3 cell)
    {
        return pathCells.Contains(cell);
    }

    private bool TrySpawnTiberiumAt(IntVec3 cell)
    {
        var p = cell.GetPlant(Map);
        if (p != null)
            p.DeSpawn();
        if (!cell.Standable(Map))
            return false;

        var terrain = cell.GetTerrain(Map);
        if (GenTiberium.AnyCorruptedOutcomes(TiberiumCrystal, terrain, out TerrainSupport support))
        {
            if (!terrain.IsTiberiumTerrain())
                Map.terrainGrid.SetTerrain(cell, support.TerrainOutcome);
            var tib = GenTiberium.Spawn(support.CrystalOutcome, this, cell, Map);
            return true;
        }

        return false;
    }

    private void SetEvolution()
    {
        if (def.evolutions.NullOrEmpty())
            return;

        var nullChance = 1 - def.evolutions.Sum(e => e.chance);
        if (TRUtils.Chance(nullChance))
            return;

        var rand = def.evolutions.InRandomOrder();
        for (var i = 0; i < rand.Count() - 1; i++)
        {
            var evolution = rand.ElementAt(i);
            if (TRUtils.Chance(evolution.chance))
            {
                ticksToEvolution = GenDate.TicksPerDay * evolution.days;
                evolvesTo = evolution.evolvedDef;
                return;
            }
        }

        var defaultEvol = rand.ElementAt(rand.Count() - 1);
        ticksToEvolution = GenDate.TicksPerDay * defaultEvol.days;
        evolvesTo = defaultEvol.evolvedDef;
    }

    private void SpawnEvolved(ThingDef def)
    {
        var newProd = (TiberiumProducer)ThingMaker.MakeThing(def);
        newProd.isEvolved = true;
        var map = Map;
        var pos = Position;
        DeSpawn(DestroyMode.Vanish, true);
        GenSpawn.Spawn(newProd, pos, map);
    }

    public void AddBoundCrystal(TiberiumCrystal crystal)
    {
        growingCrystals.Add(crystal);

        boundCrystals.Add(crystal);
        AddFieldCell(crystal.Position);
        //lastFieldCellCount++;
    }

    public void RemoveBoundCrystal(TiberiumCrystal crystal)
    {
        growingCrystals.Remove(crystal);
        boundCrystals.Remove(crystal);

        foreach (var vec in crystal.Position.CellsAdjacent8Way())
        {
            TiberiumCrystal cryst = null;
            if (vec.InBounds(Map) && (cryst = vec.GetTiberium(Map)) != null) growingCrystals.Add(cryst);
        }

        if (crystal.def.dead != null) RemoveFieldCell(crystal.Position);
        //lastFieldCellCount--;
    }

    private void ResetSpawnTicks()
    {
        ticksToSpawn = TRUtils.Range(def.spawner.spawnInterval);
    }

    private void ResetSporeCounter()
    {
        ticksToSpore = TRUtils.Range(def.spore.tickRange);
    }

    public override void Draw()
    {
        base.Draw();
        if (showField) GenDraw.DrawFieldEdges(FieldCells.ToList(), Color.red);
        if (showAffect) GenDraw.DrawFieldEdges(pathCells, Color.blue);
        //MapComponent_Tiberium tib = Map.GetComponent<MapComponent_Tiberium>();
        //GenDraw.DrawFieldEdges(tib.PawnCells.ToList(), Color.green);
        if (showTileIterator)
        {
            var tib = Map.GetComponent<MapComponent_Tiberium>();
            var list = new List<IntVec3>();
            list.Add(tib.currentDebugCell);
            GenDraw.DrawFieldEdges(list, Color.magenta);
            GenDraw.DrawFieldEdges(tib.IteratorTiles.ToList(), Color.magenta);
        }

        if (showPotentialField) GenDraw.DrawFieldEdges(InitialCells, Color.blue);
    }

    public override string GetInspectString()
    {
        inspectString = "";
        inspectString += "DEBUG:" + "\n";
        inspectString += "Radiuses - growRad " + GrowRadius + " - floodRad " + floodRadius + "\n";
        inspectString += "Paths: " + cellPaths.Count + " cells: " + pathCells.Count + "\n";
        inspectString += "Stop glow: " + turnOffLight + "\n";
        inspectString += "Stop growth: " + stopGrowth + "\n";
        inspectString += "Speedy growth: " + fastGrow + "\n";
        inspectString += "Tiberium crystals: " + boundCrystals.Count + "\n";
        inspectString += "Active crystals: " + growingCrystals.Count + "\n";
        inspectString += "Field size: " + FieldCells.Count + "\n";
        inspectString += "Show Iterator: " + showTileIterator;
        return inspectString;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;

        if (Prefs.DevMode && DebugSettings.godMode)
        {
            yield return new Command_Action
            {
                defaultLabel = "STOP GROWTH",
                action = delegate { stopGrowth = !stopGrowth; }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Kill Tiberium ",
                action = delegate
                {
                    var tibs = boundCrystals.ToArray();
                    foreach (var t in tibs) t.DeSpawn();
                    growingCrystals.Clear();
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: SPEED UP GROWTH",
                action = delegate { fastGrow = !fastGrow; }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Spawn " + TiberiumCrystal?.label,
                action = delegate
                {
                    SpawnTiberium();
                    ResetSpawnTicks();
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Spawn Blossom Spore",
                action = delegate
                {
                    SpawnBlossomSpore();
                    ResetSporeCounter();
                }
            };


            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show field cells",
                action = delegate { showField = !showField; }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show affected tiles",
                action = delegate { showAffect = !showAffect; }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show tile iterator",
                action = delegate { showTileIterator = !showTileIterator; }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show woke field",
                action = delegate { showPotentialField = !showPotentialField; }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Spawn Spores",
                action = delegate
                {
                    if (def != null)
                        GenTiberium.SpawnSpore(this.OccupiedRect(), def.spawner.growRadius, Map, TiberiumCrystal, this,
                            120);
                }
            };

            var count = 1000;
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Test " + count + " Particles",
                action = delegate
                {
                    for (var i = 0; i < count; i++)
                        ParticleMaker.SpawnParticleWithPath(FieldCells.RandomElement(), FieldCells.RandomElement(), Map,
                            DefDatabase<ParticleDef>.GetNamed("TiberiumParticle"));
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Make Mature",
                action = delegate { ticksToMature = 0; }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Mature 0.5 days",
                action = delegate { ticksToMature -= 30000; }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Toggle Glow",
                action = delegate
                {
                    turnOffLight = !turnOffLight;
                    boundCrystals.ToList().ForEach(c => c.BroadcastCompSignal("PowerTurnedOn"));
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: NO GROWTH",
                action = delegate { NoGrowth = !NoGrowth; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: NO SPREAD",
                action = delegate { NoSpread = !NoSpread; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: NO TERRTAIN",
                action = delegate { NoTerrain = !NoTerrain; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: NO REPRINT",
                action = delegate { NoReprint = !NoReprint; }
            };
        }
    }
}