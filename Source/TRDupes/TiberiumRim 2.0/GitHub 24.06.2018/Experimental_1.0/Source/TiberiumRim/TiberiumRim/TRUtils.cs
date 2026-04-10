using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse.AI;
using Verse;
using System.Reflection;
using Harmony;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class TRUtils
    {
        public static MethodInfo NewBlueprint;
        public static MethodInfo NewFrame;

        static TRUtils()
        {
            Type type1 = typeof(ThingDefGenerator_Buildings);
            NewBlueprint = type1.GetMethod("NewBlueprintDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
            NewFrame = type1.GetMethod("NewFrameDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static void DrawMenuSectionColor(Rect rect, int thiccness, ColorInt colorBG, ColorInt colorBorder)
        {
            GUI.color = colorBG.ToColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = colorBorder.ToColor;
            Widgets.DrawBox(rect, thiccness);
            GUI.color = Color.white;
        }

        public static string GetCurrentDate()
        {
            return GenDate.DateReadoutStringAt((long)Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.VisibleMap.Tile));
        }

        public static Dictionary<int, float> CopyDictionary(this Dictionary<int, float> dict)
        {
            Dictionary<int, float> newDict = new Dictionary<int, float>();
            foreach(int i in dict.Keys)
            {
                newDict.Add(i, dict[i]);
            }
            return newDict;
        }

        public static void SpawnTiberiumFromMapEdge(Map map, TiberiumType type, out TiberiumCrystal crystal)
        {
            TiberiumCrystalDef crystalDef = TRUtils.CrystalDefFromType(type);
            Predicate<IntVec3> validator = delegate (IntVec3 c)
            {
                Room room = c.GetRoom(map, RegionType.Set_Passable);
                return room != null && room.TouchesMapEdge;
            };
            CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Animal, out IntVec3 spawnCell);
            GenTiberiumReproduction.SetTiberiumTerrainAndType(crystalDef, spawnCell.GetTerrain(map), out TiberiumCrystalDef crystalDef2, out TerrainDef terrainDef);

            crystal = (TiberiumCrystal)GenSpawn.Spawn(crystalDef2, spawnCell, map);
            map.terrainGrid.SetTerrain(spawnCell, terrainDef);
        }

        public static TiberiumCrystalDef CrystalDefFromType(TiberiumType type)
        {
            TiberiumCrystalDef crystalDef = null;
            if (type == TiberiumType.Green || type == TiberiumType.Sludge)
            {
                crystalDef = TiberiumDefOf.TiberiumGreen;
            }
            if(type == TiberiumType.Blue)
            {
                crystalDef = TiberiumDefOf.TiberiumBlue;
            }
            if (type == TiberiumType.Red)
            {
                crystalDef = TiberiumDefOf.TiberiumRed;
            }
            return crystalDef;
        }

        public static bool IsFarAwayEnoughFromAny(this IntVec3 pos, ThingDef def, Map map, int minDist)
        {
            List<Thing> thingList = map.listerThings.AllThings.FindAll((Thing x) => x.def == def);
            foreach(Thing t in thingList)
            {
                if(t.Position.DistanceTo(pos) < minDist)
                {
                    return false;
                }
            }
            return true;
        }

        public static void MakeNewBluePrint(ThingDef def, bool isInstallBlueprint, ThingDef normalBlueprint = null)
        {
            NewBlueprint.Invoke(null, new object[] {def, isInstallBlueprint, normalBlueprint});
        }

        public static void MakeNewFrame(ThingDef def)
        {
            NewFrame.Invoke(null, new object[] {def});
        }

        public static Building GetBuilding(this CellRect cells, ThingDef def, Map map)
        {
            foreach (IntVec3 c in cells.Cells)
            {
                if (c.InBounds(map))
                {
                    Building thing = (Building)c.GetThingList(map).Find((Thing x) => x.def == def);
                    if (thing != null)
                    {
                        return thing;
                    }
                }
            }
            return null;
        }

        public static Thing GetAnyThingIn(this CellRect cells, Type type, Map map)
        {
            foreach(IntVec3 c in cells.Cells)
            {
                if (c.InBounds(map))
                {
                    Thing thing = (Building)c.GetThingList(map).Find((Thing x) => x.def.thingClass == type);
                    if(thing != null)
                    {
                        return thing;
                    }
                }
            }
            return null;
        }

        public static bool Contains(this CellRect cells, ThingDef def, Map map)
        {
            foreach(IntVec3 c in cells.Cells)
            {
                if (c.InBounds(map))
                {
                    if (c.GetFirstThing(map, def) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool FindValueFor(this Dictionary<StringWrapper, bool> dict, Predicate<StringWrapper> match)
        {
            bool result = false;
            foreach(StringWrapper sw in dict.Keys)
            {
                if(match(sw))
                {
                    dict.TryGetValue(sw, out result);
                }
            }
            return result;
        }

        public static IEnumerable<IntVec3> CellsAdjacent8Way(this IntVec3 loc)
        {
            IntVec3 center = loc;
            int minX = center.x - (1 - 1) / 2 - 1;
            int minZ = center.z - (1 - 1) / 2 - 1;
            int maxX = minX + 1 + 1;
            int maxZ = minZ + 1 + 1;
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minZ; j <= maxZ; j++)
                {
                    yield return new IntVec3(i, 0, j);
                }
            }
            yield break;
        }

        public static Thing AnyThingAt(this ThingGrid grid, IntVec3 loc, Map map, Type type)
        {
            if (loc.InBounds(map))
            {
                List<Thing> list = grid.ThingsListAt(loc);
                for(int i = 0; i < list.Count; i++)
                {
                    if(list[i].def.GetType() == type)
                    {
                        return list[i];
                    }
                }
            }
            return null;
        }

        public static TiberiumCrystalDef Named(this string def)
        {
            return DefDatabase<ThingDef>.GetNamed(def) as TiberiumCrystalDef;
        }

        public static IEnumerable<TiberiumCrystalDef> AllTiberiumTypesForHarvesters()
        {
            using (IEnumerator<ThingDef> enumerator = (from def in DefDatabase<ThingDef>.AllDefs
                                                       where def.thingClass == typeof(TiberiumCrystal)
                                                       select def).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TiberiumCrystalDef tibDef = (TiberiumCrystalDef)enumerator.Current;
                    yield return tibDef;

                }
            }
            yield break;
        }

        public static bool CanBeFound(this ThingDef thingDef, Map map)
        {
            Thing thing = null;
            thing = map.listerThings.AllThings.Find((Thing x) => x.def == thingDef);
            if (thing != null)
            {
                return true;
            }
            return false;
        }

        public static List<Thing> AllTiberiumCrystalsInList(this ListerThings things, bool getOnlyTypes = false)
        {
            List<Thing> returnList = new List<Thing>();
            foreach (Thing thing in things.AllThings)
            {
                if (thing != null)
                {
                    if (getOnlyTypes)
                    {
                        if (!returnList.ContainsDef(thing.def))
                        {
                            returnList.Add(thing);
                        }
                    }
                    else
                    if (thing.def.thingClass == typeof(TiberiumCrystal))
                    {
                        returnList.Add(thing);
                    }
                }
            }
            return returnList;
        }

        public static TiberiumCrystal ClosestTiberium(Pawn seeker, Map map, TraverseParms parms, PathEndMode peMode = PathEndMode.Touch, TiberiumCrystalDef preferredDef = null)
        {
            TiberiumCrystal result = null;

            return result;
        }

        public static TiberiumCrystal ClosestTiberium(Pawn harvester, IntVec3 root, Map map, TiberiumCrystalDef preferableDef, bool harvestable, TraverseParms traverseParms, PathEndMode peMode)
        {
            TiberiumCrystal result = null;           
            float maxValue = -1f;
            List<TiberiumCrystal> valueCheckList = new List<TiberiumCrystal>();
            valueCheckList.AddRange(harvestable ? (preferableDef != null && preferableDef.CanBeFound(map) == true ? 
                map.GetComponent<MapComponent_TiberiumHandler>().AllTiberiumCrystals.Where((TiberiumCrystal x) => x.def == preferableDef && harvester.CanReserve(x)) : 
                map.GetComponent<MapComponent_TiberiumHandler>().AllTiberiumCrystals.Where((TiberiumCrystal x) => x.Harvestable && harvester.CanReserve(x))) : 
                map.GetComponent<MapComponent_TiberiumHandler>().AllTiberiumCrystals.Where((TiberiumCrystal x) => !x.Harvestable && x.def.defName.Contains("Moss") && harvester.CanReserve(x)));

            if (harvestable)
            {
                if (harvester is Harvester)
                {
                    if (!(harvester as Harvester).harvestModeBool)
                    {
                        foreach (TiberiumCrystal crystal in valueCheckList)
                        {
                            if (crystal.HarvestValue > maxValue)
                            {
                                maxValue = crystal.HarvestValue;
                            }
                        }
                        valueCheckList.RemoveAll((TiberiumCrystal x) => x.HarvestValue < maxValue);
                    }
                }
            }

            RegionEntryPredicate entryCondition = (Region from, Region to) => to.Allows(traverseParms, true);
            Region region = root.GetRegion(map, RegionType.Set_Passable);
            if (region == null)
            {
                return null;
            }
            float maxDistSquared = 99980000f;
            float closestDistSquared = 9999999f;
            float bestPrio = float.MinValue;
            RegionProcessor regionProcessor = delegate (Region r)
            {
                if(r.portal == null && !r.Allows(traverseParms, true))
                {
                    return false;
                }
                if (!r.IsForbiddenEntirely(harvester))
                {
                    for(int i = 0; i < valueCheckList.Count; i++)
                    {
                        TiberiumCrystal listThing = valueCheckList[i];
                        if (ReachabilityWithinRegion.ThingFromRegionListerReachable(listThing, r, peMode, harvester))
                        {
                            float num = 0f;
                            if (num >= bestPrio)
                            {
                                float num2 = (float)(listThing.Position - root).LengthHorizontalSquared;
                                if ((num > bestPrio || num2 < closestDistSquared) && num2 < maxDistSquared)
                                {
                                    result = listThing;
                                    closestDistSquared = num2;
                                    bestPrio = num;
                                }
                            }
                        }
                    }
                }
                return result != null;
            };
            RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 30, RegionType.Set_Passable);           
            return result;
        }

        public static Hediff GetMutation(Pawn p)
        {
            if (p.health.hediffSet.HasHediff(TiberiumHediffDefOf.TiberiumMutationBad))
            {
                return p.health.hediffSet.hediffs.Find((Hediff x) => x.def == TiberiumHediffDefOf.TiberiumMutationBad);
            }
            else if (p.health.hediffSet.HasHediff(TiberiumHediffDefOf.TiberiumMutationGood))
            {
                return p.health.hediffSet.hediffs.Find((Hediff x) => x.def == TiberiumHediffDefOf.TiberiumMutationGood);
            }
            return null;
        }

        public static Hediff TibSickness(Pawn p)
        {
            if (p.health.hediffSet.HasHediff(TiberiumHediffDefOf.TiberiumExposure))
            {
                return p.health.hediffSet.hediffs.Find((Hediff x) => x.def == TiberiumHediffDefOf.TiberiumExposure);
            }
            else if (p.health.hediffSet.HasHediff(TiberiumHediffDefOf.TiberiumStage1))
            {
                return p.health.hediffSet.hediffs.Find((Hediff x) => x.def == TiberiumHediffDefOf.TiberiumStage1);
            }
            else if (p.health.hediffSet.HasHediff(TiberiumHediffDefOf.TiberiumStage2))
            {
                return p.health.hediffSet.hediffs.Find((Hediff x) => x.def == TiberiumHediffDefOf.TiberiumStage2);
            }
            else if (p.health.hediffSet.HasHediff(TiberiumHediffDefOf.TiberiumInfection))
            {
                return p.health.hediffSet.hediffs.Find((Hediff x) => x.def == TiberiumHediffDefOf.TiberiumInfection);
            }
            return null;
        }

        public static bool NextTo(this Thing thing, Thing closeThing)
        {
            List<Thing> thingList = new List<Thing>();
            foreach(IntVec3 cell in thing.CellsAdjacent8WayAndInside())
            {
                foreach(Thing thing2 in cell.GetThingList(thing.Map))
                {
                    if(thing2 == closeThing)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static TiberiumCrystal GetTiberium(this IntVec3 c, Map map)
        {
            List<Thing> list = map.thingGrid.ThingsListAt(c);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].def.thingClass == typeof(TiberiumCrystal))
                {
                    return (TiberiumCrystal)list[i];
                }
            }
            return null;
        }

        public static HashSet<TiberiumCrystalDef> AllTiberiumProducerTypesOnMap(Map map)
        {
            HashSet<TiberiumCrystalDef> result = new HashSet<TiberiumCrystalDef>();
            foreach(Thing thing in map.listerThings.AllThings.Where(x=> x.def.thingClass == typeof(Building_TiberiumProducer)))
            {
                foreach (TiberiumCrystalDef def in ((TiberiumProducerDef)thing.def).crystalDefs)
                {
                    result.Add(def);
                }
            }
            return result;
        }

        public static HashSet<TiberiumCrystalDef> AllTiberiumCrystalTypesOnMap(Map map)
        {
            HashSet<TiberiumCrystalDef> returnHash = new HashSet<TiberiumCrystalDef>();
            foreach(TiberiumCrystal crystal in map.GetComponent<MapComponent_TiberiumHandler>().AllTiberiumCrystals)
            {
                if (crystal != null)
                {
                    if (!returnHash.Contains(crystal.def))
                    {
                        returnHash.Add(crystal.def);
                    }
                }
            }
            return returnHash;
        }

        public static HashSet<TiberiumProducerDef> AllTiberiumProducers()
        {
            HashSet<TiberiumProducerDef> defHash = new HashSet<TiberiumProducerDef>();
            foreach(Def def in DefDatabase<Def>.AllDefs)
            {
                if(def is TiberiumProducerDef)
                {
                    defHash.Add(def as TiberiumProducerDef);
                }
            }
            return defHash;
        }

        public static bool ContainsDef(this HashSet<Thing> thingHash, ThingDef def)
        {
            foreach (Thing thing in thingHash)
            {
                if (thing.def == def)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsDef(this List<Thing> thingList, ThingDef def)
        {
            foreach(Thing thing in thingList)
            {
                if(thing.def == def)
                {
                    return true;
                }              
            }
            return false;
        }

        public static TiberiumCrystal GetOneThing(this List<TiberiumCrystal> thingList)
        {
            foreach(TiberiumCrystal t in thingList)
            {
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }

        public static void AffectPawn(Pawn pawn, TiberiumCrystalDef def, TiberiumCrystal parent, Map map)
        {
            if (!def.tiberium.isFlesh)
            {
                Infect(pawn, TiberiumRimSettings.settings.InfectionTouch, map, false);
            }
            else
            {
                Hurt(pawn, parent, map);
            }
        }

        public static void Infect(Pawn pawn, float amt, Map map, bool isGas = false)
        {
            HediffDef exposure = TiberiumHediffDefOf.TiberiumExposure;
            HediffDef addiction = TiberiumHediffDefOf.TiberiumAddiction;
            HediffDef Stage1 = TiberiumHediffDefOf.TiberiumStage1;
            HediffDef Stage2 = TiberiumHediffDefOf.TiberiumStage2;
            HediffDef Stage3 = TiberiumHediffDefOf.TiberiumInfection;
            HediffDef Immunity = TiberiumHediffDefOf.TiberiumInfusionImmunity;

            if (Immunity != null)
            {
                if (pawn.health.hediffSet.HasHediff(Immunity))
                {
                    return;
                }
            }

            HediffSet hediffs = pawn.health.hediffSet;

            if (hediffs.HasHediff(Stage1) | hediffs.HasHediff(Stage2) | hediffs.HasHediff(addiction))
            {
                return;
            }

            if (pawn.RaceProps.IsMechanoid)
            {
                return;
            }

            if (pawn.def.defName.Contains("_TBI"))
            {
                return;
            }

            if (pawn.Position.InBounds(map))
            {
                if (pawn.AnimalOrWildMan())
                {
                    HealthUtility.AdjustSeverity(pawn, exposure, +amt * 2);
                    return;
                }
                else
                if (pawn.apparel == null)
                {
                    HealthUtility.AdjustSeverity(pawn, exposure, +amt * 3);
                    return;
                }

                List<Apparel> Clothing = pawn.apparel.WornApparel;

                float protection = 0;

                int parts = 0;

                Apparel apparel = null;

                foreach (Apparel a in Clothing)
                {
                    if (a.def.defName.Contains("_TBP"))
                    {
                        if (a.HitPoints < a.MaxHitPoints * 0.15)
                        {
                            HealthUtility.AdjustSeverity(pawn, exposure, +amt / 2);
                            if (Current.Game.tickManager.TicksGame % 1250 == 0)
                            {
                                Messages.Message("MessageTiberiumSuitLeak".Translate(), new TargetInfo(pawn.Position, map, false), MessageTypeDefOf.CautionInput);
                            }
                            return;
                        }
                        parts = parts + 1;
                        apparel = a;
                    }
                    else if (protection < a.GetStatValue(StatDefOf.ArmorRating_Sharp))
                    {
                        protection = protection + a.GetStatValue(StatDefOf.ArmorRating_Sharp);
                    }
                }

                if (protection >= 0.6)
                {
                    return;
                }

                if (parts < 2)
                {
                    if (isGas)
                    {
                        if (apparel != null)
                        {
                            if (!apparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
                            {
                                HealthUtility.AdjustSeverity(pawn, exposure, +amt);
                                return;
                            }
                        }
                    }
                    HealthUtility.AdjustSeverity(pawn, exposure, +amt);
                    return;
                }
            }
        }      

        public static void Hurt(Pawn pawn, TiberiumCrystal parent, Map map)
        {
            if (Rand.Chance(0.75f))
            {
                float amt = MainTCD.MainTiberiumControlDef.VeinHitDamage;
                if (pawn.apparel == null)
                {
                    amt = amt * 3;
                }
                DamageInfo damage = new DamageInfo(DamageDefOf.Blunt, (int)amt);

                if (!pawn.def.defName.Contains("TBI") && pawn.Position.InBounds(map))
                {
                    if (!pawn.Downed)
                    {
                        pawn.TakeDamage(damage);
                    }
                }
                if (parent.Position.InBounds(map))
                {
                    Building b = parent.Position.RandomAdjacentCell8Way().GetFirstBuilding(map);
                    if (b != null && b.def == TiberiumDefOf.Veinhole_TBNS)
                    {
                        Infect(pawn, TiberiumRimSettings.settings.InfectionTouch, map, true);
                    }
                }
            }
        }
    }
}
