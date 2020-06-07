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
    /* TIBERIUM PRODUCER
     * The Tiberium Producer is the main source of Tiberium
     * Functionality:
     * Upon First Spawn:
     * Creates area of initial mutation area
     * Creates Cell-Paths for tiberium growth -> Instant on spawn, instead of procedural via tick
     * Sets time until full maturity
     * Resets all counters
     * 
     * Phases:
     * Maturing - The inital mutation area slowly grows from center to outmost cell. (CellMutator Class?)
     * Spawning Blossoms
     * Spawning Tiberium Lattice
     * 
     * */

    public class TiberiumProducer : TRBuilding
    {
        public new TiberiumProducerDef def;

        private AreaMutator areaMutator;
        private TiberiumField tiberiumField;

        //private List<CellPath> cellPaths = new List<CellPath>();

        //Values
        private int ticksUntilTiberium;
        private int ticksUntilSpore;

        //DebugSettings
        public bool fastGrow = false;
        public bool turnOffLight = false;
        private bool showField = false;
        private bool showAffect = false;
        private bool showTileIterator = false;
        private bool showPotentialField = false;

        public override string LabelCap => IsGroundZero ? base.LabelCap + " (GZ)" : base.LabelCap;

        public bool IsGroundZero { get; set; }
        public bool ShouldSpawnSpore => def.spore != null && IsGroundZero && ticksUntilSpore <= 0 && MatureEnough;
        public bool ShouldSpawnTiberium => !TiberiumTypes.EnumerableNullOrEmpty() && ticksUntilTiberium <= 0 && MatureEnough;
        //public bool ShouldEvolve => evolvesTo != null && ticksToEvolution <= 0;
        private bool MatureEnough => (IsFullyMature || areaMutator.ProgressPct >= def.spawner.minDaysToSpread / def.daysToMature);
        public bool IsFullyMature => areaMutator.Finished;
        public float GrowthRadius => IsGroundZero ? def.spawner.growRadius * 2.5f : def.spawner.growRadius;
        public float GroundZeroFactor => IsGroundZero ? 2.7f : 1f;

        public TiberiumFieldRuleset Ruleset => def?.tiberiumFieldRules;
        
        public IEnumerable<TiberiumCrystalDef> TiberiumTypes => Ruleset.crystalOptions.NullOrEmpty() ? null : Ruleset.crystalOptions.Select(t => t.thing as TiberiumCrystalDef);
        public List<IntVec3> FieldCells => tiberiumField.FieldCells;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = base.def as TiberiumProducerDef;

            if (respawningAfterLoad) return;
            //Init of saved components - Done Once
            //Try Set GroundZero
            WorldTiberiumComp.SetGroundZero(this);

            //Init Tiberium Field
            tiberiumField = new TiberiumField(this);

            //Init Tickers
            ResetTiberiumCounter();
            ResetSporeCounter();

            //AreaMutator, sets and processes the initial corruption Area
            int mutationTicks = (int)(GenDate.TicksPerDay * (def.daysToMature * GroundZeroFactor));
            areaMutator = new AreaMutator(Position, map, GrowthRadius, def.tiberiumFieldRules, mutationTicks, 1f, tiberiumField);

            //Init CellPaths for Tiberium growth
            bool Validator(IntVec3 c) => c.InBounds(map) && c.Standable(map);
            bool EndCon(IntVec3 c) => Position.DistanceTo(c) > GrowthRadius;

            void Processor(IntVec3 c)
            {
                TiberiumComp.TiberiumInfo.SetForceGrowBool(c, true);
            }
            List<IntVec3> cells = new List<IntVec3>();
            var AdjacentCells = GenAdj.CellsAdjacent8Way(this).ToList();
            for (int i = 0; i < AdjacentCells.Count - 1; i++)
            {
                if (i % 3 == 0)
                {
                    new CellPath(map, AdjacentCells[i], IntVec3.Invalid, Position, GrowthRadius, Validator, EndCon, Processor).CreatePath();
                }
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref ticksUntilTiberium, "ticksUntilTiberium");
            Scribe_Values.Look(ref ticksUntilSpore, "ticksUntilSpore");
            Scribe_Deep.Look(ref tiberiumField, "tiberiumField", this);
            Scribe_Deep.Look(ref areaMutator, "areaMutator", def.tiberiumFieldRules, tiberiumField);
        }

        public override void Tick()
        {
            base.Tick();
            if (!Spawned) return;

            SpawningTick();

            areaMutator?.Tick();
            tiberiumField?.Tick();
        }

        private void SpawningTick()
        {
            if (ShouldSpawnSpore)
            {
                if(TrySpawnBlossomSpore()) 
                    ResetSporeCounter();
            }

            if (ShouldSpawnTiberium)
            {
                SpawnTiberium();
                ResetTiberiumCounter();
            }

            if (ticksUntilTiberium > 0)
                ticksUntilTiberium--;
            if (ticksUntilSpore > 0)
                ticksUntilSpore--;
        }

        public void AddBoundCrystal(TiberiumCrystal crystal)
        {
            tiberiumField.AddTiberium(crystal);
            tiberiumField.AddFieldCell(crystal.Position, crystal.Map);
        }

        public void RemoveBoundCrystal(TiberiumCrystal crystal)
        {
            tiberiumField.RemoveTiberium(crystal);
            tiberiumField.RemoveFieldCell(crystal.Position, crystal.Map);
        }

        private void SpawnTiberium()
        {
            int spores;
            List<IntVec3> cells;
            switch (def.spawner.spawnMode)
            {
                case TiberiumSpawnMode.Direct:
                    cells = this.CellsAdjacent8WayAndInside().Where(c => c.InBounds(Map) && c.GetTiberium(Map) == null && c.GetFirstBuilding(Map) == null).ToList();
                    if (cells.Any())
                        GenTiberium.TrySpawnTiberium(cells.RandomElement(), Map, TiberiumTypes.RandomElement(), this);
                    break;
                case TiberiumSpawnMode.Spore:
                    cells = FieldCells.Where(c => c.InBounds(Map) && c.GetTiberium(Map) == null && 
                                            c.GetFirstBuilding(Map) == null && c.GetPlant(Map) == null && !c.Roofed(Map)).ToList();
                    if (cells.Any())
                        GenTiberium.SpawnSpore(this.OccupiedRect(), cells.RandomElement(), Map, TiberiumTypes.RandomElement(), this);
                    break;
                case TiberiumSpawnMode.SporeBurst:
                    spores = TRUtils.Range(def.spawner.explosionRange);
                    GenTiberium.SpawnSpore(this.OccupiedRect(), def.spawner.sporeExplosionRadius, Map, TiberiumTypes.RandomElement(), this, spores, true);
                    break;
                case TiberiumSpawnMode.SporeExplosion:
                    spores = TRUtils.Range(def.spawner.explosionRange);
                    GenTiberium.SpawnSpore(this.OccupiedRect(), def.spawner.sporeExplosionRadius, Map, TiberiumTypes.RandomElement(), this, spores, true);
                    GenExplosion.DoExplosion(this.Position, Map, 6.76f, DamageDefOf.Bomb, this);
                    break;
            }
        }

        private bool TrySpawnBlossomSpore()
        {
            //TODO: Fix Spores /def.spore missing
            return false;
            // Log.Message("Spawning Spore From " + this);
            // var dest = TiberiumComp.StructureInfo.GetBlossomDestination();
            // if (!dest.IsValid) return false;
            // var spore = GenTiberium.SpawnBlossomSpore(Position, dest, Map, def.spore.Blossom(), this);
            // //TODO: Make basic tiberium letter
            // LetterMaker.MakeLetter("Blossom Spore", "A blossom spore has appeared, and will fly to this position.", LetterDefOf.NeutralEvent, new LookTargets(spore.endCell, Map));
            // return true;
        }

        private void ResetTiberiumCounter()
        {
            if (def.spawner != null)
                ticksUntilTiberium = TRUtils.Range(def.spawner.spawnInterval);
        }

        private void ResetSporeCounter()
        {
            if(def.spore != null)
                ticksUntilSpore = TRUtils.Range(def.spore.spawnInterval);
        }

        private bool showForceGrow;
        private bool showGrowFrom;
        private bool showGrowTo;
        private bool showAffected;
        private bool showTiberium;
        public override void Draw()
        {
            base.Draw();
            areaMutator.DrawArea();
            tiberiumField.DrawField();

            //DebugDraw
            TiberiumGrid grid = TiberiumComp.TiberiumInfo.GetGrid();
            //Draw Tiberium Cells
            if(showTiberium)
                GenDraw.DrawFieldEdges(grid.tiberiumGrid.ActiveCells.ToList(), Color.red);
            //Draw CellPaths
            if(showForceGrow)
                GenDraw.DrawFieldEdges(grid.forceGrow.ActiveCells.ToList(), Color.blue);
            //Draw From
            if (showGrowFrom)
                GenDraw.DrawFieldEdges(grid.growFromGrid.ActiveCells.ToList(), Color.green);
            //Draw To
            if (showGrowTo)
                GenDraw.DrawFieldEdges(grid.growToGrid.ActiveCells.ToList(), Color.cyan);
            //Draw Affected Cells
            if (showAffected)
                GenDraw.DrawFieldEdges(grid.affectedCells.ActiveCells.ToList(), Color.magenta);
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.GetInspectString());
            if(IsGroundZero)
                sb.AppendLine("TR_GZProducer".Translate());
            sb.AppendLine(areaMutator.InspectString());
            sb.AppendLine(tiberiumField.InspectString());
            return sb.ToString().TrimStart().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            foreach (var gizmo in areaMutator.Gizmos())
            {
                yield return gizmo;
            }

            foreach (var gizmo in tiberiumField.Gizmos())
            {
                yield return gizmo;
            }

            if (!TiberiumTypes.EnumerableNullOrEmpty())
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn " + TiberiumTypes?.RandomElement().label,
                    action = delegate
                    {
                        SpawnTiberium();
                        ResetTiberiumCounter();
                    }
                };
            }

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Speed Up Growth ",
                action = delegate
                {
                    tiberiumField.DEBUGFastGrowth();
                    fastGrow = !fastGrow;
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show Tiberium",
                action = delegate { showTiberium = !showTiberium; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show ForceGrow",
                action = delegate { showForceGrow = !showForceGrow; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show GrowFrom",
                action = delegate { showGrowFrom = !showGrowFrom; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show GrowTo",
                action = delegate { showGrowTo = !showGrowTo; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show Affected",
                action = delegate { showAffected = !showAffected; }
            };
        }
    }

    /*
    public class TiberiumProducer3 : TRBuilding
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

                
                if (FieldCells.Contains(cell))
                {
                    Log.Message(this + " Contains existing cell");
                }
                

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
                TiberiumMapComp.TiberiumInfo.TiberiumGrid.SetFieldColor(cell, true, def.TiberiumValueType);
        }

        public void RemoveFieldCell(IntVec3 cell)
        {
            fieldCellsList.Remove(cell);
            foreach (var def in def.tiberiumTypes)
                TiberiumMapComp.TiberiumInfo.TiberiumGrid.SetFieldColor(cell, false, def.TiberiumValueType);
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
            var dest = TiberiumMapComp.StructureInfo.GetBlossomDestination();
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
    */
}
