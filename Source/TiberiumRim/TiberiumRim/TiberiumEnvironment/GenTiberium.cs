using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class GenTiberium
    {
        //TODO: Tiberium Pods ignore flora and tiberium garden
        //TODO: Make BlossomTrees to TiberiumGardens by default
                
        //Main Tiberium Spread
        public static bool TrySpreadTiberium(TiberiumCrystal crystal, bool insideProducer = false)
        {
            TerrainSupport support = null;
            IntVec3 hiddenTerrainPos = IntVec3.Invalid;

            bool Predicate(IntVec3 c) => crystal.def.CanSpreadTo(c, crystal.Map, out support, out hiddenTerrainPos, insideProducer, crystal.Parent);
            var AdjCells = crystal.Position.CellsAdjacent8Way();
            if(CellFinder.TryFindRandomCellNear(crystal.Position, crystal.Map, (int)crystal.def.tiberium.spreadRadius, Predicate, out IntVec3 pos, 8))
            {
                if (support == null) return false;

                Plant plant = pos.GetPlant(crystal.Map);
                if (plant != null)
                {
                    if (insideProducer)
                    {
                        Log.Message("Inside producer and on plant - despawning " + plant);
                        plant.DeSpawn();
                    }

                    else if ((crystal.TiberiumComp.TiberiumInfo.FloraGrid.growBools[pos] || TRUtils.Chance(crystal.def.tiberium.plantMutationChance)))
                    {
                        var defName = plant.def.defName;
                        var newPlant = GetTiberiumPlant(defName, out bool bloss);
                        Log.Message("Plant: " + newPlant);
                        if (crystal.def.plantTerrain != null)
                            crystal.Map.terrainGrid.SetTerrain(pos, crystal.def.plantTerrain);
                        GenSpawn.Spawn(newPlant, pos, crystal.Map);
                        return false;
                    }
                }
                Spawn(support.CrystalOutcome, crystal.Parent, pos, crystal.Map);
                return true;
            } 
            if(hiddenTerrainPos.IsValid)
            {
                crystal.Map.terrainGrid.SetTerrain(hiddenTerrainPos, support.TerrainOutcome);
            }
            return false;
        }

        public static void SpawnTiberiumAt(IntVec3 pos, TiberiumCrystalDef def)
        {

        }

        public static TRThingDef OutcomeAt(IntVec3 pos)
        {
            return null;
        }

        //Gen Flora 
        public static TRThingDef FloraAt()
        {

            return null;
        }

        public static bool HasFloraAt(IntVec3 pos)
        {

            return false;
        }

        //Terrain Checks
        public static bool IsSoil(this TerrainDef def)
        {
            return TiberiumDefOf.Soils.SupportsDef(def);
        }

        public static bool IsMoss(this TerrainDef def)
        {
            return TiberiumDefOf.Mosses.SupportsDef(def);
        }

        public static bool IsSand(this TerrainDef def)
        {
            return TiberiumDefOf.Sands.SupportsDef(def);
        }

        public static bool IsStone(this TerrainDef def)
        {
            return TiberiumDefOf.Stones.SupportsDef(def);
        }

        public static bool CellInRange(IntVec3 origin, float radius, Predicate<IntVec3> pred, out IntVec3 pos)
        {
            pos = IntVec3.Invalid;
            int square = (int)radius * (int)radius;
            int i = 0;
            while(i < square && !pred(pos))
            {
                pos = RandCellRange(origin, radius);
                i++;
            }
            return pos.IsValid;
        }

        public static IntVec3 RandCellRange(IntVec3 origin, float radius)
        {
            int off = Mathf.CeilToInt(TRUtils.Range(1, radius));
            float rad = TRUtils.Range(0, 360);
            Vector3 offset = new Vector3(off, 0, 0);
            Vector3 pos = origin.ToVector3();
            pos += Quaternion.Euler(0, rad, 0) * offset;
            return pos.ToIntVec3();
        }

        public static void SpreadTiberium(TiberiumCrystal crystal)
        {
            TerrainSupport support = null;
            IntVec3 hiddenTerrain = IntVec3.Invalid;
            bool pred(IntVec3 c) => c.InBounds(crystal.Map) && GenSight.LineOfSight(crystal.Position, c, crystal.Map) && crystal.def.CanSpreadTo(c, crystal.Map, out support, out hiddenTerrain);
            if (CellFinder.TryFindRandomCellNear(crystal.Position, crystal.Map, (int)crystal.def.tiberium.spreadRadius, pred, out IntVec3 spawnCell, 8) && support != null)
            {
                if (spawnCell.GetPlant(crystal.Map) != null && TRUtils.Chance(crystal.def.tiberium.plantMutationChance))
                {
                    SpawnTiberiumPlant(spawnCell, crystal.Map);
                    crystal.Map.terrainGrid.SetTerrain(spawnCell, support.TerrainOutcome);
                }
                else
                    Spawn(support.CrystalOutcome, crystal.Parent, spawnCell, crystal.Map);
            }
            else if (support != null)
                if (hiddenTerrain.IsValid)
                    crystal.Map.terrainGrid.SetTerrain(hiddenTerrain, support.TerrainOutcome);
        }

        public static bool SupportsBlossom(this IntVec3 c, Map map)
        {
            return c.InBounds(map) && !c.Fogged(map) && !c.Roofed(map) && c.Standable(map) && TiberiumDefOf.Soils.SupportsDef(c.GetTerrain(map));
        }

        public static bool SupportsTiberiumTerrain(this IntVec3 c, Map map)
        {
            return c.InBounds(map) && c.GetTerrain(map) is TerrainDef terrain && !terrain.HasTag("Water") && !GenTiberium.IsStone(terrain);
        }

        public static BlossomSpore SpawnBlossomSpore(IntVec3 start, IntVec3 dest, Map map, TiberiumProducerDef blossom, TiberiumProducer parent)
        {
            BlossomSpore spore = (BlossomSpore)ParticleMaker.MakeParticle(TiberiumDefOf.BlossomSpore);
            start += parent?.def.spawner.sporeOffset ?? IntVec3.Zero;
            spore.SporeSetup(blossom, parent);
            return (BlossomSpore)ParticleMaker.SpawnParticleWithPath(start, dest, map, spore);
        }

        public static TiberiumSpore SpawnSpore(CellRect startRect, IntVec3 dest, Map map, TiberiumCrystalDef type, TiberiumProducer parent, bool dustlike = false)
        {
            if (type == null)
                return null;
            IntVec3 cell = startRect.Cells.RandomElement();
            return SpawnSpore(cell, dest, map, type, parent, dustlike);
        }

        public static TiberiumSpore SpawnSpore(IntVec3 start, IntVec3 dest, Map map, TiberiumCrystalDef type, TiberiumProducer parent, bool dustlike = false)
        {
            if (type == null)
                return null;
            ParticleDef def = dustlike ? TiberiumDefOf.TiberiumDustSpore : TiberiumDefOf.TiberiumSpore;
            TiberiumSpore spore = (TiberiumSpore)ParticleMaker.MakeParticle(def);
            start += parent?.def.spawner.sporeOffset ?? IntVec3.Zero;
            spore.SporeSetup(type, parent);
            return (TiberiumSpore)ParticleMaker.SpawnParticleWithPath(start, dest, map, spore);    
        }

        public static void SpawnSpore(CellRect startRect, float radius, Map map, TiberiumCrystalDef type, TiberiumProducer parent, int sporeCount = 1, bool dustlike = false)
        {
            if (type == null)
                return;
            ParticleDef def = dustlike ? DefDatabase<ParticleDef>.GetNamed("TiberiumDustSpore") : DefDatabase<ParticleDef>.GetNamed("TiberiumSpore");
            List<IntVec3> alreadyAdded = new List<IntVec3>();
            for (int i = 0; i < sporeCount; i++)
            {
                TiberiumSpore spore = (TiberiumSpore)ParticleMaker.MakeParticle(def);
                spore.SporeSetup(type,parent);
                IntVec3 cell = startRect.Cells.RandomElement();
                cell += parent?.def.spawner.sporeOffset ?? IntVec3.Zero;
                CellFinder.TryFindRandomCellNear(cell, map, (int)radius, c => !alreadyAdded.Contains(c) && c.InBounds(map) && !c.Fogged(map) && !c.Roofed(map) && !c.GetTerrain(map).IsWater && c.Standable(map) && c.GetTiberium(map) == null, out IntVec3 dest);
                if (alreadyAdded.Contains(dest)) continue;
                alreadyAdded.Add(dest);
                ParticleMaker.SpawnParticleWithPath(cell, dest, map, spore);
            }
        }

        public static TiberiumProducerDef RandomSmallBlossom()
        {
            return TRUtils.Chance(0.75f) ? TiberiumDefOf.SmallBlossom : TiberiumDefOf.AlocasiaBlossom;
        }

        public static TiberiumProducerDef RandomBlossom()
        {
            if (TRUtils.Chance(0.05f))
                return TRUtils.Chance(0.45f) ? TiberiumDefOf.BlueBlossomTree : TiberiumDefOf.BlossomTree;
            return TRUtils.Chance(0.5f) ? TiberiumDefOf.AlocasiaBlossom : TiberiumDefOf.SmallBlossom;
        }

        public static void SetTiberiumTerrain(IntVec3 cell, Map map)
        {

        }

        public static TiberiumTerrainDef TerrainFrom(TerrainDef groundTerrain, TiberiumTerrainDef tibTerrain)
        {
            return tibTerrain.TerrainSupportFor(groundTerrain)?.TerrainOutcome;
        }

        public static TiberiumTerrainDef SetTiberiumTerrain(IntVec3 cell, Map map, TiberiumCrystalDef tiberium, float plantChance = 1f)
        {
            if (tiberium == null) return null;
            TiberiumTerrainDef terrain = null;
            if (AnyCorruptedOutcomes(tiberium, cell.GetTerrain(map), out TerrainSupport support))
            {
                if (TRUtils.Chance(plantChance))
                    SpawnTiberiumPlant(cell, map);
                terrain = support.TerrainOutcome;
                map.terrainGrid.SetTerrain(cell, support.TerrainOutcome);
            }
            return terrain;
        }

        public static void SpawnTiberiumPlant(IntVec3 cell, Map map)
        {
            Plant plant = cell.GetPlant(map);
            if (plant != null && !(plant is TiberiumPlant))
            {
                ThingDef plantDef = GetTiberiumPlant(plant.def.defName, out bool blossom);
                if (!blossom)
                {
                    Plant plant2 = GenSpawn.Spawn(plantDef, cell, map) as Plant;
                    plant2.Growth = plant.Growth * 0.5f;
                }
                else
                    GenSpawn.Spawn(plantDef, cell, map);
            }
        }

        public static ThingDef GetTiberiumPlant(string plantDefName, out bool blossom)
        {
            ThingDef plant = null;
            blossom = false;
            plantDefName = plantDefName.ToLower();
            if (plantDefName.Contains("tree"))
            {
                /*
                if (TRUtils.Chance(0.12f))
                {
                    blossom = true;
                    return RandomSmallBlossom();
                }*/
                plant = TiberiumDefOf.TiberiumTree;
            }
            else if (plantDefName.Contains("bush") && TRUtils.Chance(0.70f))
            {
                if (TRUtils.Chance(0.2f))
                    plant = TiberiumDefOf.TiberiumShroom_Purple;
                if (TRUtils.Chance(0.4f))
                    plant = TiberiumDefOf.TiberiumShroom_Blue;
                if (TRUtils.Chance(0.6f))
                    plant = TiberiumDefOf.TiberiumShroom_Yellow;
                plant = TiberiumDefOf.TiberiumBush;
            }
            else
            {
                plant = TiberiumDefOf.TiberiumGrass;
            }
            return plant;
        }

        public static bool IsTiberiumPlant(this Plant plant)
        {
            return plant is TiberiumPlant;
        }

        public static TiberiumCrystal Spawn(TiberiumCrystalDef def, TiberiumProducer parent, IntVec3 loc, Map map)
        {
            TiberiumCrystal newCrystal = ThingMaker.MakeThing(def) as TiberiumCrystal;
            if (parent != null)
                newCrystal.PreSpawnSetup(parent, false, 0);

           
            TiberiumCrystal tib = loc.GetTiberium(map);
            if (tib != null)
            {
                //tib.DeSpawn();
                return null;
            }

            return GenSpawn.Spawn(newCrystal, loc, map) as TiberiumCrystal;
        }

        public static TiberiumCrystal TryGetTiberiumFor(this IntVec3 c, Harvester harvester)
        {
            TiberiumCrystal crystal = c.GetTiberium(harvester.Map);
            if (crystal?.CanBeHarvestedBy(harvester) ?? false)
                return crystal;
            return null;
        }

        public static TiberiumCrystal GetTiberium(this IntVec3 c, Map map)
        {
            if (map == null || !c.InBounds(map))
                return null;

            return map.GetComponent<MapComponent_Tiberium>().TiberiumInfo.TiberiumGrid
                    .TiberiumCrystals[map.cellIndices.CellToIndex(c)];
        }

        public static bool CanSpreadTo(this TiberiumCrystalDef def, IntVec3 c, Map map, out TerrainSupport support, out IntVec3 hiddenTerrain, bool insideProducer = false, TiberiumProducer producer = null)
        {
            support = null;
            hiddenTerrain = IntVec3.Invalid;

            bool inPath = producer?.InsideGrowPath(c) ?? true;
            if (!c.IsValid || !c.InBounds(map) || !c.Standable(map) || (insideProducer && !inPath))
                return false;

            var list = c.GetThingList(map);
            if (list.Any(t => t.def.designateHaulable))
            {
                hiddenTerrain = c;
                return false;
            }

            if (list.Any(t => t.def.IsEdifice() || t is Building || t is TiberiumCrystal || (t is TiberiumPlant && !inPath)))
                return false;

            TerrainDef terrain = c.GetTerrain(map);
            if (!AnyCorruptedOutcomes(def, terrain, out support))
                return false;

            return true;
        }

        public static bool AnyCorruptedOutcomes(this TiberiumCrystalDef c, TerrainDef t, out TerrainSupport s)
        {
            return (s = c.TerrainSupportFor(t)) != null;
        }

        /*
        public static TiberiumTerrainDef CorruptedOutcomesFor(this TiberiumCrystalDef crystalDefIn, TerrainDef terrainIn, out TiberiumCrystalDef crystalDefOut, bool forProducer = false)
        {
            string terrain = terrainIn.defName.ToLower();
            crystalDefOut = null;
            if (!forProducer && crystalDefIn.tiberium.corruptsWater)
            {
                if (terrainIn.IsWater)
                {
                    crystalDefOut = TiberiumDefOf.TiberiumGlacier;
                    if (terrainIn.IsRiver)
                    {
                        if (terrain.Contains("deep"))
                        {
                            return TiberiumTerrainDefOf.TiberiumWaterMovingChestDeep;
                        }
                        return TiberiumTerrainDefOf.TiberiumShallowMovingWater;
                    }
                    if (terrain.Contains("deep"))
                    {
                        return TiberiumTerrainDefOf.TiberiumDeepWater;
                    }
                    return TiberiumTerrainDefOf.TiberiumShallowWater;
                }
                if (terrain.Contains("mud"))
                {
                    if (crystalDefIn.soil != null)
                    {
                        crystalDefOut = crystalDefIn;
                        return crystalDefIn.soil;
                    }
                }
            }
            if (terrain.Contains("soil") || terrain.Contains("marshy") || terrain.Contains("gravel") || terrain.Contains("dirt"))
            {
                if (crystalDefIn.soil != null)
                {
                    crystalDefOut = crystalDefIn;
                    return crystalDefIn.soil;
                }
            }
            if (!forProducer && IsStone(terrain))
            {
                if (crystalDefIn.stone != null)
                {
                    crystalDefOut = crystalDefIn.stoneType;
                    return crystalDefIn.stone;
                }
            }
            if (!IsStone(terrain) && terrain.Contains("sand"))
            {
                if (crystalDefIn.dry != null)
                {
                    crystalDefOut = crystalDefIn.dryType;
                    return crystalDefIn.dry;
                }
            }
            if (terrain.Contains("mossy"))
            {
                if (crystalDefIn.mossy != null)
                {
                    crystalDefOut = crystalDefIn.stoneType;
                    return crystalDefIn.mossy;
                }
            }
            if (terrain.Contains("ice"))
            {
                if (crystalDefIn.ice != null)
                {
                    crystalDefOut = crystalDefIn;
                    return crystalDefIn.ice;
                }
            }
            return null;
        }
        */
    }
}
