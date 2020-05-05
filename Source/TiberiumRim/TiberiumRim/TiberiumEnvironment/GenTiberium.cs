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
        public static bool TrySpreadTiberium(TiberiumCrystal crystal)
        {
            //TODO: Predetermine grow-to cells, with canspreadto, select all adjacent growto cells, select random weighted cell, with weight away from the producer
            Predicate<IntVec3> predicate = c => TrySpawnTiberium(c, crystal.Map, crystal.def, crystal.Parent);
            if (CellFinder.TryFindRandomCellNear(crystal.Position, crystal.Map, (int) crystal.def.tiberium.spreadRadius, predicate, out IntVec3 result))
            {
                return true;
            }
            /*
            var surroundingCells = GenRadial.RadialCellsAround(crystal.Position, crystal.def.tiberium.spreadRadius, false).InRandomOrder();
            foreach (var cell in surroundingCells)
            {
                if (TrySpawnTiberium(cell, crystal.Map, crystal.def, crystal.Parent))
                    return true;
            }
            */
            return false;
        }

        public static bool CanSpreadTo(this TiberiumCrystalDef def, IntVec3 pos, Map map, out IntVec3 hiddenTerrainPos, out bool needsFlora)
        {
            hiddenTerrainPos = IntVec3.Invalid;
            needsFlora = false;
            //IntVec3 Check
            if (!pos.Standable(map)) return false;

            //Flora Check
            if (ShouldGrowFloraAt(pos, map))
            {
                needsFlora = true;
                return false;
            }

            //GrowToCheck
            if (!CanGrowTo(pos, map)) return false;

            //Object Check
            var thingList = pos.GetThingList(map);
            if (thingList.Any(t => t.def.designateHaulable))
            {
                hiddenTerrainPos = pos;
                return false;
            }

            if (thingList.Any(t => t is TiberiumCrystal || (t is TiberiumPlant && !ForceGrowAt(pos, map)))) return false;
            return def.HasOutcomesAt(pos, map);
        }

        public static bool TryMutatePlant(Plant plant, TiberiumCrystalDef def)
        {
            var map = plant.Map;
            var position = new IntVec3(plant.Position.x, plant.Position.y, plant.Position.z);
            var tibComp = map.Tiberium();
            if (!(tibComp.FloraInfo.ShouldGrowFloraAt(position) || TRUtils.Chance(def.tiberium.plantMutationChance))) return false;
            if (!def.FloraOutcomesFor(plant.def, out TRThingDef toPlant, out TerrainDef floraTerrain)) return false;
            //var newPlant = SelectTiberiumPlant(plant.def, out bool blossom);
            //if(!blossom && floraTerrain != null)

            map.terrainGrid.SetTerrain(position, floraTerrain);
            GenSpawn.Spawn(toPlant, position, map);
            return true;
        }

        public static bool TrySpawnTiberium(IntVec3 pos, Map map, TiberiumCrystalDef def, TiberiumProducer parent = null)
        {
            if (!pos.IsValid || !pos.InBounds(map)) return false;

            //Prepare potential outcomes for the desired spread position
            def.SpreadOutcomesAt(pos, map, out TerrainDef topTerrain, out TerrainDef underTerrain, out TiberiumCrystalDef crystalDef);

            //Check if the tiberium can spread to the position
            if (!CanSpreadTo(def, pos, map, out IntVec3 hidden, out bool needsFlora))
            {
                if (needsFlora)
                {
                    //TODO: Tiberium Garden Creation
                    GenSpawn.Spawn(TiberiumDefOf.TiberiumShroom_Purple, pos, map);
                }
                else if (!hidden.IsValid) return false;

                SetTerrain(pos, map, topTerrain, underTerrain);
                return false;
            }
            var plant = pos.GetPlant(map);
            if (plant != null && TryMutatePlant(plant, def)) return false;

            SetTerrain(pos, map, topTerrain, underTerrain);
            Spawn(pos, map, crystalDef, parent);
            return true;
        }

        //TODO: Create "PlantSupport"
        public static ThingDef SelectTiberiumPlant(ThingDef plantDef, out bool blossom)
        {
            ThingDef plant;
            blossom = false;
            var defName = plantDef.defName.ToLower();
            if (defName.Contains("tree"))
            {
                /*
                if (TRUtils.Chance(0.12f))
                {
                    blossom = true;
                    return RandomSmallBlossom();
                }*/
                plant = TiberiumDefOf.TiberiumTree;
            }
            else if (defName.Contains("bush") && TRUtils.Chance(0.70f))
            {
                if (TRUtils.Chance(0.2f))
                    plant = TiberiumDefOf.TiberiumShroom_Purple;
                else if (TRUtils.Chance(0.4f))
                    plant = TiberiumDefOf.TiberiumShroom_Blue;
                else if (TRUtils.Chance(0.6f))
                    plant = TiberiumDefOf.TiberiumShroom_Yellow;
                else
                    plant = TiberiumDefOf.TiberiumBush;
            }
            else
                plant = TiberiumDefOf.TiberiumGrass;
            return plant;
        }


        public static TiberiumCrystal Spawn(IntVec3 pos, Map map, TiberiumCrystalDef def, TiberiumProducer parent = null)
        {
            TiberiumCrystal newCrystal = (TiberiumCrystal)ThingMaker.MakeThing(def);
            if (parent != null)
                newCrystal.PreSpawnSetup(parent, 0);
            return GenSpawn.Spawn(newCrystal, pos, map) as TiberiumCrystal;
        }

        public static TiberiumCrystal GetTiberium(this IntVec3 pos, Map map)
        {
            if (!pos.InBounds(map)) return null;
            return map.Tiberium().TiberiumInfo.TiberiumAt(pos); 
        }

        public static TiberiumCrystal TryGetTiberiumFor(this IntVec3 c, Harvester harvester)
        {
            TiberiumCrystal crystal = c.GetTiberium(harvester.Map);
            if (crystal?.CanBeHarvestedBy(harvester) ?? false)
                return crystal;
            return null;
        }

        public static void GetTerrain(this IntVec3 pos, Map map, out TerrainDef top, out TerrainDef under)
        {
            top = map.terrainGrid.TerrainAt(pos);
            under = map.terrainGrid.UnderTerrainAt(pos);
        }

        public static void SetTerrain(IntVec3 pos, Map map, TerrainDef topTerrain, TerrainDef underTerrain)
        {
            if (topTerrain != null)
                map.terrainGrid.SetTerrain(pos, topTerrain);
            if (underTerrain != null)
                map.terrainGrid.SetUnderTerrain(pos, underTerrain);
        }

        public static void SetTerrain(IntVec3 pos, Map map, TiberiumCrystalDef def)
        {
            def.SpreadOutcomesAt(pos, map, out TerrainDef topTerrain, out TerrainDef underTerrain, out TiberiumCrystalDef crystalDef);
            if (topTerrain != null)
                map.terrainGrid.SetTerrain(pos, topTerrain);
            if (underTerrain != null)
                map.terrainGrid.SetUnderTerrain(pos, underTerrain);
        }

        //Tiberium Spores
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
                spore.SporeSetup(type, parent);
                IntVec3 cell = startRect.Cells.RandomElement();
                cell += parent?.def.spawner.sporeOffset ?? IntVec3.Zero;
                CellFinder.TryFindRandomCellNear(cell, map, (int)radius, c => !alreadyAdded.Contains(c) && c.InBounds(map) && !c.Fogged(map) && !c.Roofed(map) && !c.GetTerrain(map).IsWater && c.Standable(map) && c.GetTiberium(map) == null, out IntVec3 dest);
                if (alreadyAdded.Contains(dest)) continue;
                alreadyAdded.Add(dest);
                ParticleMaker.SpawnParticleWithPath(cell, dest, map, spore);
            }
        }

        //Static Bools n' Checks
        public static bool CanBeHarvestedBy(this TiberiumCrystal crystal, Harvester harvester)
        {
            //TODO: Multiple reservations
            if (!crystal.Map.reservationManager.CanReserve(harvester, crystal))
                return false;

            var def = crystal.def;
            if (def.HarvestType == HarvestType.Unharvestable)
                return false;

            return harvester.harvestMode switch
            {
                HarvestMode.Nearest => !def.IsMoss,
                HarvestMode.Value => (def == crystal.TiberiumMapComp.TiberiumInfo.MostValuableType),
                HarvestMode.Moss => def.IsMoss,
                _ => false
            };
        }

        public static bool IsTiberiumPlant(this Plant plant)
        {
            return plant is TiberiumPlant;
        }

        //Support Bools
        public static bool SupportsTiberiumTerrain(this IntVec3 c, Map map)
        {
            return c.InBounds(map) && c.GetTerrain(map) is TerrainDef terrain && !terrain.HasTag("Water") && !terrain.IsStone();
        }

        public static bool SupportsBlossom(this IntVec3 c, Map map)
        {
            return c.InBounds(map) && !c.Fogged(map) && !c.Roofed(map) && c.Standable(map) && TiberiumDefOf.TerrainFilter_Soil.AllowsTerrainDef(c.GetTerrain(map));
        }

        //Grid Bools

        public static bool HasTiberium(IntVec3 c, Map map)
        {
            return map.Tiberium().TiberiumInfo.HasTiberiumAt(c);
        }

        public static bool CanGrowFrom(IntVec3 c, Map map)
        {
            return map.Tiberium().TiberiumInfo.CanGrowFrom(c);
        }

        public static bool CanGrowTo(IntVec3 c, Map map)
        {
            return map.Tiberium().TiberiumInfo.CanGrowTo(c);
        }

        public static bool IsAffectedCell(IntVec3 c, Map map)
        {
            return map.Tiberium().TiberiumInfo.IsAffectedCell(c);
        }

        public static bool ForceGrowAt(IntVec3 c, Map map)
        {
            return map.Tiberium().TiberiumInfo.ForceGrowAt(c);
        }

        public static bool IsSuppressed(IntVec3 c, Map map)
        {
            return map.Tiberium().Suppression.IsInSuppressorField(c);
        }

        public static bool HasFloraAt(IntVec3 c, Map map)
        {
            return map.Tiberium().FloraInfo.HasFloraAt(c);
        }

        public static bool ShouldGrowFloraAt(IntVec3 c, Map map)
        {
            return map.Tiberium().FloraInfo.ShouldGrowFloraAt(c);
        }

        //Terrain Checks
        public static bool IsSoil(this TerrainDef def)
        {
            return TiberiumDefOf.TerrainFilter_Soil.AllowsTerrainDef(def);
        }

        public static bool IsMoss(this TerrainDef def)
        {
            return TiberiumDefOf.TerrainFilter_Moss.AllowsTerrainDef(def);
        }

        public static bool IsSand(this TerrainDef def)
        {
            return TiberiumDefOf.TerrainFilter_Sand.AllowsTerrainDef(def);
        }

        public static bool IsStone(this TerrainDef def)
        {
            return TiberiumDefOf.TerrainFilter_Stone.AllowsTerrainDef(def);
        }
    }

    public static class GenTiberium2
    {
        //TODO: Tiberium Pods ignore flora and tiberium garden
        //TODO: Make BlossomTrees to TiberiumGardens by default
                
        /*Main Tiberium Spread
        public static bool TrySpreadTiberium(TiberiumCrystal crystal, bool insideProducer = false)
        {
            TerrainSupport support = null;
            IntVec3 hiddenTerrainPos = IntVec3.Invalid;

            bool Predicate(IntVec3 c) => crystal.def.CanSpreadTo(c, crystal.Map, out support, out hiddenTerrainPos, insideProducer, crystal.Parent);
            if(CellFinder.TryFindRandomCellNear(crystal.Position, crystal.Map, (int)crystal.def.tiberium.spreadRadius, Predicate, out IntVec3 pos, 8))
            {
                if (support == null) return false;
                Plant plant = pos.GetPlant(crystal.Map);
                if (plant != null)
                {
                    if (insideProducer)
                    {
                        plant.DeSpawn();
                    }
                    else if ((crystal.TiberiumMapComp.TiberiumInfo.FloraGrid.growBools[pos] || TRUtils.Chance(crystal.def.tiberium.plantMutationChance)))
                    {
                        var defName = plant.def.defName;
                        var newPlant = GetTiberiumPlant(defName, out bool bloss);
                        if (crystal.def.plantTerrain != null)
                            crystal.Map.terrainGrid.SetTerrain(pos, crystal.def.plantTerrain);
                        GenSpawn.Spawn(newPlant, pos, crystal.Map);
                        return false;
                    }
                }
                Spawn(support.CrystalOutcome, crystal.Parent, pos, crystal.Map);

                return true;
            }
            if (hiddenTerrainPos.IsValid && support != null)
            {
                crystal.Map.terrainGrid.SetTerrain(hiddenTerrainPos, support.TerrainOutcome);
            }
            return false;
        }
        */


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

        //Blossoms
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




    }
}
