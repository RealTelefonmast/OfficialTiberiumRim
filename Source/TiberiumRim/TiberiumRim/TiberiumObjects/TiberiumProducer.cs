using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{

    public class CellPath : IExposable
    {
        public Map map;
        public IntVec3 start;
        public IntVec3 end;

        public IntVec3 pusher;
        public float growRadius;


        private readonly List<IntVec3> pathCells = new List<IntVec3>();
        private readonly Action<IntVec3> processor;
        private readonly Predicate<IntVec3> predicate;

        private IntVec3 nextCell;
        private IntVec3 lastCell;
        private float lastDist;
        private int attempts = 0;
        private bool finished = false;

        public CellPath() { }

        public CellPath(Map map, IntVec3 start, IntVec3 end, IntVec3 pusher, float growRadius, Predicate<IntVec3> endCondition, Action<IntVec3> processor = null)
        {
            this.map = map;
            this.start = start;
            this.end = end;
            this.pusher = pusher;
            this.growRadius = growRadius;
            this.predicate = endCondition;
            this.processor = processor;

            if (pusher.IsValid)
                lastDist = pusher.DistanceTo(start);

            nextCell = start;
        }

        public void ExposeData()
        {
            
        }

        public void Grow(float radius, ref List<IntVec3> cells)
        {
            for (;;)
            {
                if (lastDist >= radius || attempts > 8) break;
                Grow(ref cells);
            }
        }

        public void Grow(int amount, ref List<IntVec3> cells)
        {
            for (int i = 0; i < amount; i++)
            {
                Grow(ref cells);
            }
        }

        public void Grow(ref List<IntVec3> cells)
        {
            if (pusher.IsValid)
            {
                IntVec3 cell = GrowAway();
                if (cell.IsValid)
                {
                    cells.Add(cell);
                    pathCells.Add(cell);
                }
            }
            else if (end.IsValid)
            {
                GrowTo();
            }
        }

        private IntVec3 GrowAway()
        {
            float dist = pusher.DistanceTo(nextCell);
            var curDist = lastDist;
            lastDist = dist;

            if ((predicate != null && predicate(nextCell)) || dist >= growRadius)
                return IntVec3.Invalid;

            if (dist >= curDist && nextCell.InBounds(map) && nextCell.Standable(map) && !pathCells.Contains(nextCell))
            {
                lastCell = nextCell;
                nextCell = nextCell.RandomAdjacentCell8Way();
                attempts = 0;
                return lastCell;
            }
            nextCell = lastCell.RandomAdjacentCell8Way();
            attempts++;
            return IntVec3.Invalid;
        }

        private void GrowTo()
        {

        }

        public List<IntVec3> CurrentPath => pathCells;
    }

    public class TiberiumProducer : TRBuilding, IResearchTarget
    {
        public new TiberiumProducerDef def;
        public List<TiberiumCrystal> boundCrystals = new List<TiberiumCrystal>();
        public List<TiberiumCrystal> growingCrystals = new List<TiberiumCrystal>();
        //TODO: Replace cell lists with areas 
        //TODO: Make custom area class
        private HashSet<IntVec3> fieldCellsList = new HashSet<IntVec3>();

        //Ticker
        private int ticksToSpawn = 100;
        private int ticksToSpore = 100;
        private int ticksToMature = 0;
        private int ticksToEvolution = 0;

        //Maturing
        public  List<IntVec3> InitialCells = new List<IntVec3>();
        private List<CellPath> cellPaths = new List<CellPath>();
        private List<IntVec3> pathCells = new List<IntVec3>();
        private float floodRadius = 0;
        private float curRadius = 0;
        private bool isEvolved = false;
        private TiberiumProducerDef evolvesTo;

        //Ground Zero
        public Building researchCrane;
        private bool isGroundZero = false;

        //Debug
        public bool NoSpread = false;
        public bool NoTerrain = false;
        public bool NoGrowth = false;
        public bool NoReprint = false;

        public bool fastGrow = false;
        public bool stopGrowth = false;
        public bool stopTicking = false;
        public bool turnOffLight = false;
        private bool showField = false;
        private bool showAffect = false;
        private bool showTileIterator = false;
        private bool showPotentialField = false;

        protected int foo = 1234;

        public override string Label
        {
            get
            {
                if (isGroundZero)
                    return base.Label + " " + "(GZ)";
                return base.Label;
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
            {
                //We USED TO guess a random amount of field cells, if respawning
                //SetPotentialFieldCells();
                return;
            }
            // First Time Spawn //
            //Setting up values
            ResetSpawnTicks();
            if(def.spore != null)
                ResetSporeCounter();

            //If possible, setup potential evolution
            SetEvolution();

            //If this is the first producer, make it Ground Zero
            WorldTiberiumComp.SetupGroundZero(this, Map, ref isGroundZero);

            if (isEvolved)
                return;

            ticksToMature = (int) (GenDate.TicksPerDay * def.daysToMature);
            //Setting up initial cells, may take a while thus making it a long event
            SetInitialCells();
            //LongEventHandler.QueueLongEvent(SetInitialCells, "SettingInitialProducerCells", false, null);

            var AdjacentCells = GenAdj.CellsAdjacent8Way(this).ToList();
            bool EndCond(IntVec3 c) => IsMature && Position.DistanceTo(c) >= floodRadius;
            for (int i = 0; i < AdjacentCells.Count - 1; i++)
            {
                if (i % 3 == 0)
                    cellPaths.Add(new CellPath(map, AdjacentCells[i], IntVec3.Invalid, Position, floodRadius, EndCond));
            }

            foreach (var adjCell in AdjacentCells.Where(c => c.InBounds(map)))
            {
                if (!def.tiberiumTerrain.NullOrEmpty())
                {
                    var terrain = adjCell.GetTerrain(Map);
                    var terr = def.tiberiumTerrain.Find(t => t.TerrainSupportFor(terrain) != null);
                    if (terr == null) continue;
                    var newTerr = GenTiberium.TerrainFrom(terrain, terr);
                    Map.terrainGrid.SetTerrain(adjCell, newTerr);
                    
                }
                else
                    GenTiberium.SetTiberiumTerrain(adjCell, Map, TiberiumCrystal);
            }
        }
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            this.DeSpawn(mode, false);
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
            int radialCellCount = GenRadial.NumCellsInRadius(GrowRadius);
            bool Predicate(IntVec3 c) => c.SupportsTiberiumTerrain(Map);
            void Action(IntVec3 c)
            {
                InitialCells.Add(c);
                float curDist = c.DistanceTo(Position);
                if (curDist > floodRadius)
                    floodRadius = curDist;
            }
            TiberiumFloodInfo flood = new TiberiumFloodInfo(Map, Predicate, Action);
            flood.TryMakeFlood(out List<IntVec3> cells, this.OccupiedRect(), radialCellCount);
        }

        public void DeSpawn(DestroyMode mode, bool replace)
        {
            if (!replace)
            {
                var killVer = def.killedVersion;
                if (killVer != null)
                    GenSpawn.Spawn(killVer, this.Position, Map);

                if (def.leaveTiberium)
                {
                    int amt = 6;
                    for (int i = 0; i < amt; i++)
                    {
                        TiberiumCrystal crystal = ThingMaker.MakeThing(TiberiumCrystal) as TiberiumCrystal;
                        Predicate<IntVec3> pred = c => c.GetTiberium(Map) == null;
                        GenPlace.TryPlaceThing(crystal, Position, Map, ThingPlaceMode.Near, null, pred);
                    }
                }
            }
            base.DeSpawn();
        }

        private int tickLeft = 750;

        public override void Tick()
        {
            base.Tick();
            if (!Spawned)
                return;

            if (fastGrow)
            {
                for (int i = growingCrystals.Count - 1; i >= 0; i--)
                {
                    growingCrystals[i].TiberiumTick();
                }

            }

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
            {
                for (int j = 0; j < 150; j++)
                {
                    for (int i = growingCrystals.Count - 1; i >= 0; i--)
                    {
                        growingCrystals[i].TiberiumTick();
                    }
                }
            }
            
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
            float radius = Mathf.Lerp(0f, floodRadius, WokePercent);
            curRadius = radius;
            float radialCount = GenRadial.NumCellsInRadius(radius);
            //var cells = InitialCells.Where(c => c.DistanceTo(Position) <= radius);
            for (int i = 0; i < radialCount; i++)
            {
                var cell = Position + GenRadial.RadialPattern[i];
                if (!InitialCells.Contains(cell)) continue;

                InitialCells.Remove(cell);
                TerrainDef terrain = cell.GetTerrain(Map);

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
                    if(newTerr != null)
                        Map.terrainGrid.SetTerrain(cell, newTerr);
                }
                if (newTerr == null && !def.customTerrain.NullOrEmpty())
                {
                    newTerr = def.customTerrain.Find(s => s.TerrainTag.SupportsDef(terrain)).TerrainOutcome;
                }
                if (newTerr == null)
                    newTerr = GenTiberium.SetTiberiumTerrain(cell, Map, TiberiumCrystal);

                if (newTerr != null && def.growsFlora && cell.Standable(Map) && cell.GetFirstBuilding(Map) == null)
                    TrySpreadFlora(cell, newTerr);
            }
        }

        private void GrowCellPaths()
        {
            foreach (var path in cellPaths)
            {
                path.Grow(curRadius, ref pathCells);
            }
        }

        public void TrySpreadFlora(IntVec3 pos, TiberiumTerrainDef terrain)
        {
            if (pos.GetPlant(Map) is TiberiumPlant) return;
            float distance = Position.DistanceTo(pos);
            float chance = 1f - Mathf.InverseLerp(0f, floodRadius, distance);;
            if (TRUtils.Chance(chance * terrain.plantChanceFactor))
            {
                ThingDef flora = SelectedFloraAt(distance, terrain);
                if (flora != null)
                {
                    Thing plant = ThingMaker.MakeThing(flora);
                    if(plant is Plant p)
                        p.Growth = TRUtils.Range(0.1f, 0.55f);
                    GenSpawn.Spawn(plant, pos, Map);
                }
            }
        }

        private ThingDef SelectedFloraAt(float distance, TiberiumTerrainDef terrain)
        {
            return def.SelectPlantByDistance(distance, floodRadius, terrain);
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

        public HashSet<IntVec3> FieldCells
        {
            get
            {
                return fieldCellsList;
            }
        }

        public float GrowRadius => isGroundZero ? def.spawner.growRadius * 2.5f : def.spawner.growRadius;

        public float WokePercent => 1f - (float)ticksToMature / (def.daysToMature * (float)GenDate.TicksPerDay);

        public bool ShouldSpawnSpore => isGroundZero && ticksToSpore <= 0 && MatureEnough;
        public bool ShouldSpawn => def.tiberiumTypes.Any() && ticksToSpawn <= 0 && MatureEnough;
        public bool ShouldEvolve => evolvesTo != null && ticksToEvolution <= 0;
        private bool MatureEnough => (IsMature || ticksToMature < def.spawner.minDaysToSpread * GenDate.TicksPerDay);
        public bool IsMature => ticksToMature <= 0 && !InitialCells.Any();

        public bool ResearchBound
        {
            get
            {
                if (researchCrane == null)
                {
                    researchCrane = (Building)Map.thingGrid.ThingAt(Position, TiberiumDefOf.TiberiumResearchCrane);
                }
                return !researchCrane.DestroyedOrNull();
            }
        }

        private void SpawnBlossomSpore()
        {
            var dest = TiberiumComp.StructureInfo.GetBlossomDestination();
            if (!dest.IsValid) return;
            var spore = GenTiberium.SpawnBlossomSpore(Position, dest, Map, def.spore.Blossom(), this);
            LetterMaker.MakeLetter("Blossom Spore", "A blossom spore has appeared, and will fly to this position.", LetterDefOf.NeutralEvent, new LookTargets(spore.endCell, Map));
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
                        c.InBounds(Map) && c.GetTiberium(Map) == null && c.GetFirstBuilding(Map) == null && c.GetPlant(Map) == null &&
                        !c.Roofed(Map)).ToList();
                    if (cells.Any())
                        GenTiberium.SpawnSpore(this.OccupiedRect(), cells.RandomElement(), Map, TiberiumCrystal, this);
                    break;
                case TiberiumSpawnMode.SporeBurst:
                    spores = TRUtils.Range(def.spawner.explosionRange);
                    GenTiberium.SpawnSpore(this.OccupiedRect(), def.spawner.sporeExplosionRadius, Map, TiberiumCrystal, this,
                        spores, true);
                    break;
                case TiberiumSpawnMode.SporeExplosion:
                    spores = TRUtils.Range(def.spawner.explosionRange);
                    GenTiberium.SpawnSpore(this.OccupiedRect(), def.spawner.sporeExplosionRadius, Map, TiberiumCrystal, this,
                        spores, true);
                    GenExplosion.DoExplosion(this.Position, Map, 6.76f, DamageDefOf.Bomb, this);
                    break;
            }
        }

        public bool InsideGrowPath(IntVec3 cell)
        {
            return pathCells.Contains(cell);
        }

        private bool TrySpawnTiberiumAt(IntVec3 cell)
        {
            Plant p = cell.GetPlant(Map);
            if(p != null)
                p.DeSpawn();
            if (!cell.Standable(Map))
                return false;

            var terrain = cell.GetTerrain(Map);
            if (GenTiberium.AnyCorruptedOutcomes(TiberiumCrystal, terrain, out TerrainSupport support))
            {
                if(!terrain.IsTiberiumTerrain())
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

            float nullChance = 1 - def.evolutions.Sum(e => e.chance);
            if (TRUtils.Chance(nullChance))
                return;

            var rand = def.evolutions.InRandomOrder();
            for (int i = 0; i < rand.Count() - 1; i++)
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
            TiberiumProducer newProd = (TiberiumProducer)ThingMaker.MakeThing(def);
            newProd.isEvolved = true;
            var map = Map;
            var pos = Position;
            this.DeSpawn(DestroyMode.Vanish, true);
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
                if (vec.InBounds(Map) && (cryst = vec.GetTiberium(Map)) != null)
                {
                    growingCrystals.Add(cryst);
                }
            }
            if (crystal.def.dead != null)
            {
                RemoveFieldCell(crystal.Position);
                //lastFieldCellCount--;
            }
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
            if (showField)
            {
                GenDraw.DrawFieldEdges(FieldCells.ToList(), Color.red);
            }
            if (showAffect)
            {
                GenDraw.DrawFieldEdges(pathCells, Color.blue);
                //MapComponent_Tiberium tib = Map.GetComponent<MapComponent_Tiberium>();
                //GenDraw.DrawFieldEdges(tib.PawnCells.ToList(), Color.green);
            }
            if (showTileIterator)
            {
                MapComponent_Tiberium tib = Map.GetComponent<MapComponent_Tiberium>();
                List<IntVec3> list = new List<IntVec3>();
                list.Add(tib.currentDebugCell);
                GenDraw.DrawFieldEdges(list, Color.magenta);
                GenDraw.DrawFieldEdges(tib.IteratorTiles.ToList(), Color.magenta);
            }
            if (showPotentialField)
            {
                GenDraw.DrawFieldEdges(InitialCells, Color.blue);
            }
        }


        private string inspectString = "";
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
            foreach(Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            if (Prefs.DevMode && DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "STOP GROWTH",
                    action = delegate
                    {
                        stopGrowth = !stopGrowth;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Kill Tiberium ",
                    action = delegate
                    {
                        var tibs = boundCrystals.ToArray();
                        foreach (var t in tibs)
                        {
                            t.DeSpawn();                           
                        }
                        growingCrystals.Clear();
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: SPEED UP GROWTH",
                    action = delegate
                    {
                        fastGrow = !fastGrow;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn " + TiberiumCrystal?.label,
                    action = delegate
                    {
                        this.SpawnTiberium();
                        this.ResetSpawnTicks();
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
                    action = delegate
                    {
                        showField = !showField;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Show affected tiles",
                    action = delegate
                    {
                        showAffect = !showAffect;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Show tile iterator",
                    action = delegate
                    {
                        showTileIterator = !showTileIterator;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Show woke field",
                    action = delegate
                    {
                        showPotentialField = !showPotentialField;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn Spores",
                    action = delegate
                    {
                        if (def != null)
                        {
                            GenTiberium.SpawnSpore(this.OccupiedRect(), def.spawner.growRadius, Map, TiberiumCrystal, this, 120);
                        }
                    }
                };

                int count = 1000;
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Test " + count + " Particles",
                    action = delegate
                    {
                        for (int i = 0; i < count; i++)
                            ParticleMaker.SpawnParticleWithPath(FieldCells.RandomElement(), FieldCells.RandomElement(), Map, DefDatabase<ParticleDef>.GetNamed("TiberiumParticle"));
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Make Mature",
                    action = delegate
                    {
                        ticksToMature = 0;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Mature 0.5 days",
                    action = delegate
                    {
                        ticksToMature -= 30000;
                    }
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
                    action = delegate
                    {
                        NoGrowth = !NoGrowth;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: NO SPREAD",
                    action = delegate
                    {
                        NoSpread = !NoSpread;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: NO TERRTAIN",
                    action = delegate
                    {
                        NoTerrain = !NoTerrain;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: NO REPRINT",
                    action = delegate
                    {
                        NoReprint = !NoReprint;
                    }
                };
            }
        }
    }
}
