using System;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace TiberiumRim
{
    public static class GenTiberiumReproduction
    {
        public static TiberiumCrystal TryReproduceFrom(IntVec3 source, TiberiumCrystal crystal, SeedTargFindMode mode, Map map, MapComponent_TiberiumHandler mapComp, List<ThingDef> friendlyTo)
        {
            if (!TryFindCorruptionDestination(source, crystal.def, mode, map, friendlyTo, out IntVec3 dest))
            {
                return null;
            }
            return TryReproduceInto(dest, crystal, map, mapComp);
        }

        public static TiberiumCrystal TryReproduceInto(IntVec3 dest, TiberiumCrystal crystal, Map map, MapComponent_TiberiumHandler mapComp)
        {
            TiberiumCrystalDef crystalDef = crystal.def;

            if (!crystalDef.CanEverGrowTo(dest, map))
            {
                return null;
            }
            if (!GenPlant.SnowAllowsPlanting(dest, map))
            {
                return null;
            }
            if (TiberiumRimSettings.settings.UseSpreadRadius)
            {
                if (!mapComp.ForcedAllowedCells.Contains(dest))
                {
                    return null;
                }
            }

            SetTiberiumTerrainAndType(crystalDef, dest.GetTerrain(map), out crystalDef, out TerrainDef setTerrain);

            if (Rand.Chance(0.8f) && crystalDef != null)
            {
                if (crystal.boundProducer != null)
                {
                    TiberiumCrystal newCrystal = (TiberiumCrystal)GenSpawn.Spawn(crystal, dest, map);
                    newCrystal.boundProducer = crystal.boundProducer;
                    crystal.boundProducer.boundCrystals.Add(newCrystal);
                    return newCrystal;
                }             
                return (TiberiumCrystal)GenSpawn.Spawn(crystalDef, dest, map);
            }
            return null;
        }

        public static void SetTiberiumTerrainAndType(TiberiumCrystalDef crystalDefIn, TerrainDef terrainDef, out TiberiumCrystalDef crystalDefOut, out TerrainDef terrainDefOut)
        {
            crystalDefOut = null;
            terrainDefOut = null;
            if (terrainDef == crystalDefIn.defaultTerrain)
            {
                terrainDefOut = crystalDefIn.defaultTerrain;
                crystalDefOut = crystalDefIn;
                return;
            }
            if (crystalDefIn.tiberium.corruptsWater)
            {
                if (IsShallowWater(terrainDef))
                {
                    crystalDefOut = TiberiumDefOf.TiberiumGlacier;
                    terrainDefOut = TerrainDef.Named("TiberiumShallowWater");
                    return;
                }
                if (IsDeepWater(terrainDef))
                {
                    crystalDefOut = TiberiumDefOf.TiberiumGlacier;
                    terrainDefOut = TerrainDef.Named("TiberiumDeepWater");
                    return;
                }
            }
            if (IsSoil(terrainDef))
            {
                if (crystalDefIn.tiberium.corruptsSoil)
                {
                    crystalDefOut = crystalDefIn;
                    terrainDefOut = crystalDefIn.defaultTerrain;
                }
                return;
            }
            if (IsMossy(terrainDef))
            {
                if (crystalDefIn.tiberium.corruptsSoil)
                {
                    crystalDefOut = crystalDefIn;
                    terrainDefOut = crystalDefIn.defaultTerrain;
                    return;
                }

                if (crystalDefIn.tiberium.corruptsStone)
                {
                    crystalDefOut = crystalDefIn.stoneType;
                    if (crystalDefIn.mossyTerrain != null)
                    {
                        terrainDefOut = crystalDefIn.mossyTerrain;
                    }
                }
                return;
            }
            if (IsSand(terrainDef))
            {
                if (crystalDefIn.tiberium.corruptsSand)
                {
                    crystalDefOut = crystalDefIn.sandType;
                    if (crystalDefIn.sandTerrain != null)
                    {
                        terrainDefOut = crystalDefIn.sandTerrain;
                        return;
                    }
                }
                return;
            }
            if (IsStoneOrDead(terrainDef))
            {
                if (crystalDefIn.tiberium.corruptsStone)
                {
                    crystalDefOut = crystalDefIn.stoneType;
                    if (crystalDefIn.stoneTerrain != null)
                    {
                        terrainDefOut = crystalDefIn.stoneTerrain;
                        return;
                    }
                }
                return;
            }   
            if(terrainDef is TiberiumTerrainDef)
            {
                crystalDefOut = crystalDefIn;
            }
        }

        //Terrain Checks

        public static bool IsShallowWater(TerrainDef def)
        {
            if ((def.defName.Contains("Water") || def.defName.Contains("Marsh")) && !def.defName.Contains("Deep"))
            {
                return true;
            }
            return false;
        }

        public static bool IsDeepWater(TerrainDef def)
        {
            if(def.defName.Contains("Water") && def.defName.Contains("Deep"))
            {
                return true;
            }
            return false;
        }

        public static bool IsSoil(TerrainDef def)
        {
            if((def == TerrainDefOf.Soil || def.defName.Contains("Soil") || def == TerrainDefOf.Gravel) && !def.defName.Contains("Marsh"))
            {   
                return true;
            }
            return false;
        }

        public static bool IsSand(TerrainDef def)
        {
            if(def == TerrainDefOf.Sand || def.defName.Contains("Sand") && !def.defName.Contains("Sandstone"))
            {
                return true;
            }
            return false;
        }

        public static bool IsMossy(TerrainDef def)
        {
            if(def.defName.Contains("Marshy") || def.defName.Contains("Moss"))
            {
                return true;
            }
            return false;
        }

        public static bool IsStoneOrDead(TerrainDef def)
        {
            TiberiumTerrainDef tibDef = def as TiberiumTerrainDef;
            if (!IsShallowWater(def) || !IsDeepWater(def))
            {
                if (tibDef == null)
                {
                    if (def.fertility <= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void ChangeTerrain(IntVec3 c, Map map, TerrainDef setTerrain)
        {
            if (c.InBounds(map))
            {
                TerrainDef targetTerrain = c.GetTerrain(map);
                map.terrainGrid.SetTerrain(c, setTerrain);
            }
        }

        public static ThingDef GetAnyPlant(String name)
        {
            ThingDef plant = null;
            if (name.Contains("Tree"))
            {
                plant = TiberiumDefOf.TiberiumTree;

            }
            else if (name.Contains("Bush") && Rand.Chance(0.65f))
            {
                if (Rand.Chance(0.45f))
                {
                    plant = TiberiumDefOf.TiberiumBush;
                }
                else
                {
                    plant = TiberiumDefOf.TiberiumShroom;
                }
            }
            else
            {
                plant = TiberiumDefOf.TiberiumGrass;
            }
            return plant;
        }

        // ----

        public static bool TryFindCorruptionDestination(IntVec3 source, TiberiumCrystalDef crystalDef, SeedTargFindMode mode, Map map, List<ThingDef> friendlyTo, out IntVec3 foundCell)
        {
            float radius = -1f;
            if (mode == SeedTargFindMode.Reproduce)
            {
                radius = crystalDef.tiberium.reproduceRadius;
            }
            else if (mode == SeedTargFindMode.MapEdge)
            {
                radius = 40f;
            }
            Predicate<IntVec3> validator = (IntVec3 c) => crystalDef.CanEverGrowTo(c, map) && GenPlant.SnowAllowsPlanting(c, map) && source.InHorDistOf(c, radius) && GenSight.LineOfSight(source, c, map, true, null, 0, 0);
            return CellFinder.TryFindRandomCellNear(source, map, Mathf.CeilToInt(radius), validator, out foundCell);
        }
    }
}
