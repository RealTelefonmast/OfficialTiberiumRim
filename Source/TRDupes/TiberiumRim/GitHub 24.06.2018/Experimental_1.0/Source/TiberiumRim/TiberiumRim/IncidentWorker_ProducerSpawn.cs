using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class IncidentWorker_ProducerSpawn : IncidentWorker
    {
        public new TiberiumIncidentDef def;
        public IntVec3 spawnCell = IntVec3.Invalid;

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            this.def = (TiberiumIncidentDef)base.def;
            Map map = (Map)target;
            StringWrapper defName = def.asteroidType.defName;
            if (TiberiumRimSettings.settings.UseProducerCap)
            {
                if(!(TiberiumRimSettings.settings.TiberiumProducersAmt > map.GetComponent<MapComponent_TiberiumHandler>().ProducerCount))
                {
                    return false;
                }
            }
            if (TiberiumRimSettings.settings.UseSpecificProducers)
            {
                if (!TiberiumRimSettings.settings.ProducerBools.FindValueFor(sw => sw.value == defName))
                {
                    return false;
                }
            }
            if (def.appears)
            {
                if (!TryFindRootCell(map, out spawnCell))
                {
                    return false;
                }
            }
            else
            {
                if (!TryFindCell(out spawnCell, map))
                {
                    return false;
                }
            }
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntRange intRange = new IntRange(10, 20);

            if (!CanFireNowSub(parms.target))
            {
                return false;
            }
            if (!Find.World.GetComponent<WorldComponent_TiberiumSpread>().tiberiumSpawned)
            {
                DiaNode diaNode = new DiaNode("TiberiumArrivalDesc".Translate());
                DiaOption diaOption = new DiaOption(". . .");
                diaOption.action = delegate
                {
                    SpawnProducer(map, intRange);
                };
                diaOption.resolveTree = true;
                diaNode.options.Add(diaOption);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, "TiberiumArrivalTitle".Translate()));
                Find.World.GetComponent<WorldComponent_TiberiumSpread>().tiberiumSpawned = true;
                return true;
            }
            if (SpawnProducer(map, intRange))
            {
                return true;
            }
            return false;
        }

        private bool SpawnProducer(Map map, IntRange intRange)
        {
            if (def.appears)
            {
                if (def.spawnCrystals)
                {
                    if (def.tiberiumType == null)
                    {
                        Log.Error("Missing TiberiumType");
                    }
                    List<TiberiumCrystal> tempList = new List<TiberiumCrystal>();
                    Thing thing = null;
                    int randomInRange = intRange.RandomInRange;
                    for (int i = 0; i < randomInRange; i++)
                    {
                        TiberiumCrystalDef finalCrystalDef = null;
                        if (!CellFinder.TryRandomClosewalkCellNear(spawnCell, map, (def.asteroidType as TiberiumProducerDef).terrainRadius, out IntVec3 intVec, delegate (IntVec3 x)
                        {
                            if (CanSpawnAt(x, map))
                            {
                                GenTiberiumReproduction.SetTiberiumTerrainAndType(def.tiberiumType, x.GetTerrain(map), out finalCrystalDef, out TerrainDef spawnTerrain);
                                if (finalCrystalDef != null)
                                {
                                    return true;
                                }
                            }
                            return false;

                        })) { break; }
                        TiberiumCrystal crystal = intVec.GetTiberium(map);
                        if (crystal != null)
                        {
                            crystal.Destroy(DestroyMode.Vanish);
                        }
                        Thing thing2 = GenSpawn.Spawn((finalCrystalDef as ThingDef), intVec, map);
                        if ((def.asteroidType as TiberiumProducerDef).bindsToCrystals)
                        {
                            tempList.Add(thing2 as TiberiumCrystal);
                        }
                        if (thing == null)
                        {
                            thing = thing2;
                        }
                    }
                    if (thing == null)
                    {
                        return false;
                    }
                    Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, new TargetInfo(spawnCell, map, false), null);
                    Thing spawnedThing = GenSpawn.Spawn(def.asteroidType, spawnCell, map);
                    (spawnedThing as Building_TiberiumProducer).boundCrystals.AddRange(tempList);
                    return true;
                }
            }
            else
            {
                SkyfallerMaker.SpawnSkyfaller(def.skyfallerDef, def.asteroidType, spawnCell, map);
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, new TargetInfo(spawnCell, map, false), null);
            }
            return true;
        }

        private bool TryFindCell(out IntVec3 cell, Map map)
        {
            TiberiumIncidentDef LocalDef = this.def as TiberiumIncidentDef;
            return CellFinderLoose.TryFindSkyfallerCell(LocalDef.skyfallerDef, map, out cell, 13, default(IntVec3), -1, true, false, false, false, delegate (IntVec3 x)
            {
                if (x.InBounds(map))
                {
                    TiberiumCrystal crystal = x.GetTiberium(map);
                    TerrainDef terrain = x.GetTerrain(map);

                    if (crystal != null)
                    {
                        return false;
                    }
                    if (terrain != null)
                    {
                        if (terrain.defName.Contains("Water") || terrain.defName.Contains("Marsh"))
                        {
                            return false;
                        }
                    }
                    if (x.Fogged(map))
                    {
                        return false;
                    }
                    foreach (IntVec3 current in GenAdj.CellsOccupiedBy(x, Rot4.North, new IntVec2(4, 4)))
                    {
                        if (!current.Standable(map) || map.roofGrid.Roofed(current) || current.GetFirstBuilding(map) != null)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            });
        }

        private bool TryFindRootCell(Map map, out IntVec3 cell)
        {
            TiberiumIncidentDef LocalDef = (TiberiumIncidentDef)this.def;
            cell = IntVec3.Invalid;
            if (LocalDef.sourceThing != null)
            {
                Thing sourceThing = map.listerThings.AllThings.Find((Thing x) => x.def.defName.Contains(def.sourceThing) && x.Position.DistanceToEdge(map) >= 13);
                if (sourceThing != null)
                {
                    cell = sourceThing.Position;
                    if (cell.InBounds(map))
                    {
                        return true;
                    }
                }
            }
            else
            {
                return CellFinderLoose.TryFindRandomNotEdgeCellWith(12, delegate (IntVec3 x) 
                {
                    if(x.InBounds(map))
                    if(CanSpawnAt(x, map))
                    {
                        TerrainDef terrain = x.GetTerrain(map);
                        if(terrain != null)
                        {
                            if (!(GenTiberiumReproduction.IsShallowWater(terrain) || GenTiberiumReproduction.IsDeepWater(terrain)))
                            {
                                return true;
                            }
                        }
                    }
                    return false;                    
                }, map, out cell);
            }
            return false;
        }

        private bool CanSpawnAt(IntVec3 x, Map map)
        {
            bool result = false;
            foreach (IntVec3 c in GenAdj.CellsOccupiedBy(x, Rot4.North, new IntVec2(4, 4)))
            {
                if (!c.Standable(map) || c.Fogged(map) || !c.GetRoom(map, RegionType.Set_Passable).PsychologicallyOutdoors || c.GetEdifice(map) != null)
                {
                    result = false;
                }       
            }
            result = true;
            return result;
        }
    }
}
