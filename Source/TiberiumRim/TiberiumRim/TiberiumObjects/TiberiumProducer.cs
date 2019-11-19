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
    public class TiberiumProducer : TRBuilding
    {
        public new TiberiumProducerDef def;
        public List<TiberiumCrystal> boundCrystals = new List<TiberiumCrystal>();
        public HashSet<IntVec3> FieldCells = new HashSet<IntVec3>();
        private int lastFieldCells = 0;
        private bool isGroundZero = false;

        //Ticker
        private int ticksToSpawn = 0;
        private int ticksToSpore = 0;
        private int ticksToMature = 0;
        private int ticksToEvolution = 0;

        //Maturing
        public List<IntVec3> InitialCells = new List<IntVec3>();
        private float floodRadius = 0;
        private bool isEvolved = false;
        private TiberiumProducerDef evolvesTo;

        //Ground Zero Story
        public Building researchCrane;
        public bool researchDone = false;

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastFieldCells, "lastFieldCells");
            Scribe_Values.Look(ref ticksToSpawn, "ticksToSpawn");
            Scribe_Values.Look(ref ticksToSpore, "ticksToSpore");
            Scribe_Values.Look(ref ticksToMature, "ticksToMature");
            Scribe_Values.Look(ref ticksToEvolution, "ticksToEvolution");
            Scribe_Values.Look(ref isGroundZero, "isGroundZero");
            Scribe_Values.Look(ref researchDone, "researchDone");
            Scribe_Values.Look(ref floodRadius, "floodRadius");
            Scribe_Collections.Look(ref InitialCells, "InitCells");
            Scribe_Defs.Look(ref evolvesTo, "evolvesTo");
        }

        public override string Label
        {
            get
            {
                if(isGroundZero)
                    return base.Label + " " + "(GZ)";
                return base.Label;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = base.def as TiberiumProducerDef;

            SetPotentialFieldCells();

            if (respawningAfterLoad) return;
            ResetTiberiumCounter();
            if(def.spore != null)
                ResetSporeCounter();
            SetEvolution();
            WorldTiberiumComp.SetupGroundZero(this, Map, ref isGroundZero);
            if (isEvolved)
                return;

            ticksToMature = (int) (GenDate.TicksPerDay * def.daysToMature);
            SetInitialCells();
            foreach (IntVec3 cell in this.OccupiedRect())
            {
                if (!def.tiberiumTerrain.NullOrEmpty())
                {
                    var terrain = cell.GetTerrain(Map);
                    var terr = def.tiberiumTerrain.Find(t => t.TerrainSupportFor(terrain) != null);
                    var newTerr = GenTiberium.TerrainFrom(terrain, terr);
                    if(newTerr == null)
                        continue;
                    Map.terrainGrid.SetTerrain(cell, newTerr);
                }
                else
                    GenTiberium.SetTiberiumTerrain(cell, Map, TiberiumCrystal);
            }
        }

        private void SetPotentialFieldCells()
        {
            if (def.spawner.growRadius <= 0) return;
            FloodFiller floodFill = new FloodFiller(Map);
            float max = def.spawner.growRadius;
            Predicate<IntVec3> predicate = x => x.GetTerrain(Map) is TiberiumTerrainDef && x.DistanceTo(Position) <= max;
            Action<IntVec3> action = delegate(IntVec3 c)
            {
                FieldCells.Add(c);
            };
            var num = lastFieldCells > 0 ? lastFieldCells : 2147483647;
            floodFill.FloodFill(Position, predicate, action, num);
        }

        private void SetInitialCells()
        {
            var growRad = def.spawner.growRadius;
            growRad *= isGroundZero ? 2.5f : 1;
            int num = GenRadial.NumCellsInRadius(growRad);
            bool Valid(IntVec3 c) => c.SupportsTiberiumTerrain(Map);
            TiberiumFloodInfo flood = new TiberiumFloodInfo(Map, num, Valid, null);
            if (flood.TryMakeFlood(out List<IntVec3> cells, this.OccupiedRect(), true, num))
            {
                InitialCells.AddRange(cells);
                floodRadius = cells.Max(c => c.DistanceTo(Position));
            }
        }

        public void DeSpawn(DestroyMode mode = DestroyMode.Vanish, bool evolving = false)
        {
            if (!evolving)
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

        public override void Tick()
        {
            base.Tick();
            if (!Spawned)
                return;

            if (ResearchBound)
            {
                if (!researchDone) return;
                if (TRUtils.Chance(0.1f))
                    researchCrane.TakeDamage(new DamageInfo(DamageDefOf.Mining, 5, 1, -1, this));
            }

            if (fastGrow)
                Find.CameraDriver.StartCoroutine(FastGrow());

            if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
            {
                if (!IsMature)
                    SpreadTerrain();
                if (ShouldSpawnSpore)
                {
                    SpawnBlossomSpore();
                    ResetSporeCounter();
                }
                if (ShouldSpawn)
                {
                    SpawnTiberium();
                    ResetTiberiumCounter();
                }
                if (ShouldEvolve)
                    SpawnEvolved(evolvesTo);
            }

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

        public void SpreadTerrain()
        {
            float radius = Mathf.Lerp(0f, floodRadius, WokePercent);
            var cells = InitialCells.Where(c => c.DistanceTo(Position) <= radius);
            for (int i = cells.Count() - 1; i >= 0; i--)
            {
                var cell = cells.ElementAt(i);
                InitialCells.Remove(cell);
                TerrainDef terrain = cell.GetTerrain(Map);
                if (FieldCells.Contains(cell)) continue;
                FieldCells.Add(cell);
                lastFieldCells++;
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

        public IEnumerator FastGrow()
        {
            foreach (TiberiumCrystal crystal in boundCrystals)
            {
                crystal.TickLong();
            }
            yield return null;
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

        public float WokePercent => 1f - (float)ticksToMature / (def.daysToMature * (float)GenDate.TicksPerDay);

        public bool ShouldSpawnSpore => isGroundZero && ticksToSpore <= 0 && MatureEnough;
        public bool ShouldSpawn => (!def.forResearch || researchDone) && def.tiberiumTypes.Any() && ticksToSpawn <= 0 && MatureEnough;
        public bool ShouldEvolve => evolvesTo != null && ticksToEvolution <= 0;
        private bool MatureEnough => (IsMature || ticksToMature < def.spawner.minDaysToSpread * GenDate.TicksPerDay);
        public bool IsMature => ticksToMature <= 0 && !InitialCells.Any();

        public bool ResearchBound
        {
            get
            {
                if (!researchDone && researchCrane == null)
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
                        c.InBounds(Map) && c.GetTiberium(Map) == null && c.GetFirstBuilding(Map) == null && c.GetPlant(Map) == null).ToList();
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

        private bool TrySpawnTiberiumAt(IntVec3 cell)
        {
            if (!cell.Standable(Map))
                return false;

            if (GenTiberium.AnyCorruptedOutcomes(TiberiumCrystal, cell.GetTerrain(Map), out TerrainSupport support))
            {
                if(!cell.GetTerrain(Map).IsTiberiumTerrain())
                    Map.terrainGrid.SetTerrain(cell, support.TerrainOutcome);
                var tib = GenTiberium.Spawn(support.CrystalOutcome, this, cell, Map);
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
            boundCrystals.Add(crystal);
            FieldCells.Add(crystal.Position);
            lastFieldCells++;
        }

        public void RemoveBoundCrystal(TiberiumCrystal crystal)
        {
            boundCrystals.Remove(crystal);
            if (crystal.def.dead != null)
            {
                FieldCells.Remove(crystal.Position);
                lastFieldCells--;
            }
        }

        private void ResetTiberiumCounter()
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

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            if (DebugSettings.godMode)
            {
                sb.AppendLine("DEBUG:");
                sb.AppendLine("Stop glow: " + turnOffLight);
                sb.AppendLine("Stop growth: " + stopGrowth);
                sb.AppendLine("Speedy growth: " + fastGrow);
                sb.AppendLine("Tiberium crystals: " + boundCrystals.Count);
                sb.AppendLine("Field size: " + FieldCells.Count);
                sb.AppendLine("Show Iterator: " + showTileIterator);
            }
            return sb.ToString().TrimEndNewlines();
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
                        this.ResetTiberiumCounter();
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
