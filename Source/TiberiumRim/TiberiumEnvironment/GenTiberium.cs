using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public static class GenTiberium
    {
        public static bool TrySpreadTiberium(TiberiumCrystal crystal)
        {
            var crystalDef = crystal.def;
            if (crystal.Parent != null) 
                crystalDef = crystal.Parent.TiberiumCrystalDefWeighted;
            bool Predicate(IntVec3 c) => TrySpawnTiberium(c, crystal.Map, crystalDef, crystal.Parent);
            return GenAdj.CellsAdjacent8Way(crystal).InRandomOrder().Any(Predicate);
        }

        public static bool AllowsTiberiumAtFast(IntVec3 cell, Map map)
        {
            if (HasTiberium(cell, map) || HasTibFlora(cell, map)) return false;
            return true;
        }

        public static bool AllowsTiberiumAt(IntVec3 cell, Map map, TiberiumCrystalDef def, out bool requiresFlora)
        {
            requiresFlora = false;
            if (!CanGrowTo(cell, map)) return false;

            //Flora Check
            if (ShouldGrowFloraAt(cell, map))
            {
                if (HasTibFlora(cell, map)) return false;
                requiresFlora = true;
                return false;
            }

            var thingList = cell.GetThingList(map);
            foreach (var thing in thingList)
            {
                if (MainTCD.Main.spreadFilter.Contains(thing.def)) return false;
                if (thing.def.passability != Traversability.Standable) return false;
                //if (thing.def.IsBuilding()) return false;
                //if (thing.def.designateHaulable) return false;
            }
            return true;
        }

        //Used when attempting to spawn tiberium at a random position
        public static bool TrySpawnTiberium(IntVec3 pos, Map map, TiberiumCrystalDef def, TiberiumProducer parent = null)
        {
            if (!pos.IsValid || !pos.InBounds(map)) return false;

            //Prepare potential outcomes for the desired spread position
            if (!def.TryGetTiberiumOutcomesAt(pos, map, out TerrainDef topTerrain, out TerrainDef underTerrain, out TiberiumCrystalDef crystalDef)) 
                return false;

            //Check if the tiberium can spread to the position
            if (!AllowsTiberiumAt(pos, map, def, out bool requiresFlora))
            {
                if (requiresFlora && pos.Standable(map))
                {
                    var flora = parent?.Ruleset.PlantAt(0.25f, 1);
                    GenSpawn.Spawn(flora ?? TiberiumDefOf.TiberiumGrass, pos, map);
                }
                return false;
            }

            //SetTerrain(pos, map, topTerrain, underTerrain);
            Spawn(pos, map, crystalDef, parent);
            return true;
        }

        //Eidi blatt 1
        private static void Test(int[] values)
        {
            int max = int.MinValue;
            int size = values.Length + 1;
            
            for (int i = size; i > 0; i--)
            {
                if (max > values[i])
                    max = values[i];

            }
        }

        //Used when the position is known and valid
        public static TiberiumCrystal SpawnTiberium(IntVec3 pos, Map map, TiberiumCrystalDef def, TiberiumProducer parent = null, bool withTerrain = false)
        {
            if (pos.GetTiberium(map) != null) return null;
            def.GetTiberiumOutcomesAt(pos, map, out TerrainDef topTerrain, out TerrainDef underTerrain, out TiberiumCrystalDef crystalDef);
            if(withTerrain) 
                SetTerrain(pos, map, topTerrain, underTerrain);

            return crystalDef == null ? null : Spawn(pos, map, crystalDef, parent);
        }

        //Spawns Tiberium directly at the position
        public static TiberiumCrystal Spawn(IntVec3 pos, Map map, TiberiumCrystalDef def, TiberiumProducer parent = null)
        {
            var newCrystal = (TiberiumCrystal)ThingMaker.MakeThing(def);
            if (parent != null)
                newCrystal.PreSpawnSetup(parent, 0);
            return GenSpawn.Spawn(newCrystal, pos, map) as TiberiumCrystal;
        }

        //Flora and plant stuff
        public static bool TryMutatePlant(Plant plant, TiberiumCrystalDef def)
        {
            if (plant == null) return false;
            //if (!TRandom.Chance(props.tiberium.plantMutationChance)) return false;
            var map = plant.Map;
            var position = new IntVec3(plant.Position.x, plant.Position.y, plant.Position.z);

            //var shouldGrowFlora = tibComp.FloraInfo.ShouldGrowFloraAt(position);
            //if (!(shouldGrowFlora))
                //return false;

            var hasOut = def.GetFloraOutcomes(plant.def, out ThingDef toPlant, out TerrainDef floraTerrain);
            if (!hasOut) return false;
            var newPlant = (TiberiumPlant)ThingMaker.MakeThing(toPlant);
            newPlant.Growth += Rand.Range(0, plant.Growth);

            map.terrainGrid.SetTerrain(position, floraTerrain);
            plant.DeSpawn();
            GenSpawn.Spawn(newPlant, position, map);
            return true;
        }

        //Getters
        public static TiberiumCrystal GetTiberium(this IntVec3 pos, Map map)
        {
            if (map == null) return null;
            if (!pos.InBounds(map)) return null;
            return map.Tiberium().TiberiumInfo.TiberiumAt(pos); 
        }

        public static TiberiumCrystal TryGetTiberiumFor(this IntVec3 c, Harvester harvester)
        {
            var crystal = c.GetTiberium(harvester.Map);
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
            def.GetTiberiumOutcomesAt(pos, map, out TerrainDef topTerrain, out TerrainDef underTerrain, out TiberiumCrystalDef crystalDef);
            if (topTerrain != null)
                map.terrainGrid.SetTerrain(pos, topTerrain);
            if (underTerrain != null)
                map.terrainGrid.SetUnderTerrain(pos, underTerrain);
        }

        //Tiberium Spores //TODO: Reimplement based on flecks
        /*
        public static BlossomSpore SpawnBlossomSpore(IntVec3 start, IntVec3 dest, Map map, TiberiumProducerDef blossom, TiberiumProducer parent)
        {
            BlossomSpore spore = (BlossomSpore)ParticleMaker.MakeParticle(TiberiumDefOf.BlossomSpore);
            start += parent?.props.spawner.sporeOffset ?? IntVec3.Zero;
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
            ParticleDef props = dustlike ? TiberiumDefOf.TiberiumDustSpore : TiberiumDefOf.TiberiumSpore;
            TiberiumSpore spore = (TiberiumSpore)ParticleMaker.MakeParticle(props);
            start += parent?.props.spawner.sporeOffset ?? IntVec3.Zero;
            spore.SporeSetup(type, parent);
            return (TiberiumSpore)ParticleMaker.SpawnParticleWithPath(start, dest, map, spore);
        }

        public static void SpawnSpore(CellRect startRect, float radius, Map map, TiberiumCrystalDef type, TiberiumProducer parent, int sporeCount = 1, bool dustlike = false)
        {
            if (type == null)
                return;
            ParticleDef props = dustlike ? DefDatabase<ParticleDef>.GetNamed("TiberiumDustSpore") : DefDatabase<ParticleDef>.GetNamed("TiberiumSpore");
            List<IntVec3> alreadyAdded = new List<IntVec3>();
            for (int i = 0; i < sporeCount; i++)
            {
                TiberiumSpore spore = (TiberiumSpore)ParticleMaker.MakeParticle(props);
                spore.SporeSetup(type, parent);
                IntVec3 cell = startRect.Cells.RandomElement();
                cell += parent?.props.spawner.sporeOffset ?? IntVec3.Zero;
                CellFinder.TryFindRandomCellNear(cell, map, (int)radius, c => !alreadyAdded.Contains(c) && c.InBounds(map) && !c.Fogged(map) && !c.Roofed(map) && !c.GetTerrain(map).IsWater && c.Standable(map) && c.GetTiberium(map) == null, out IntVec3 dest);
                if (alreadyAdded.Contains(dest)) continue;
                alreadyAdded.Add(dest);
                ParticleMaker.SpawnParticleWithPath(cell, dest, map, spore);
            }
        }
        */

        public static TiberiumProducerDef BlossomTreeFrom(TiberiumProducerDef producer)
        {
            var crystals = producer.tiberiumFieldRules.crystalOptions;
            if (crystals.NullOrEmpty()) return TiberiumDefOf.BlossomTree;
            var crystalType = (crystals.RandomElement().def).TiberiumValueType;
            if (crystalType == TiberiumValueType.Green)
                return TiberiumDefOf.BlossomTree;
            if (crystalType == TiberiumValueType.Blue)
                return TiberiumDefOf.BlueBlossomTree;
            return null;
        }

        //Static Bools n' Checks
        public static bool CanBeHarvestedBy(this TiberiumCrystal crystal, Harvester harvester)
        {
            if (!crystal.Map.reservationManager.CanReserve(harvester, crystal))
                return false;

            if (crystal.def.HarvestType == HarvestType.Unharvestable)
                return false;

            return harvester.HarvestMode switch
            {
                HarvestMode.Nearest => !crystal.def.IsMoss,
                HarvestMode.Value => (crystal.def == crystal.TiberiumMapComp.TiberiumInfo.MostValuableType),
                HarvestMode.Moss => crystal.def.IsMoss,
                _ => false
            };
        }

        public static bool IsTiberiumPlant(this Plant plant)
        {
            return plant is TiberiumPlant;
        }

        //Support Bools
        public static bool UsableForAnyTiberiumPurpose(this IntVec3 c, Map map)
        {
            //Maybe useless
            return !c.IsSuppressed(map);
        }

        public static bool AllowsTiberiumTerrain(this IntVec3 c, Map map, TiberiumCrystalDef crystalDef)
        {
            return crystalDef.conversions.HasOutcomeFor(c.GetTerrain(map), out _);
        }

        public static bool SupportsTiberiumTerrain(this IntVec3 c, Map map)
        {
            return c.InBounds(map) && c.GetTerrain(map) is TerrainDef terrain && !terrain.HasTag("Water") && !terrain.IsStone();
        }

        public static bool SupportsBlossom(this IntVec3 c, Map map)
        {
            return c.InBounds(map) && !c.Fogged(map) && !c.Roofed(map) && c.Standable(map) && TiberiumDefOf.TerrainFilter_Soil.Allows(c.GetTerrain(map));
        }

        public static bool CanSendSporeTo(this IntVec3 c, Map map, TiberiumCrystalDef def)
        {
            return !c.InBounds(map) && !c.Roofed(map) && !c.Fogged(map) &&  AllowsTiberiumAt(c, map, def, out _);
        }

        //Grid Bools
        public static bool HasTibFlora(this IntVec3 c, Map map)
        {
           return map.Tiberium().FloraInfo.HasFloraAt(c);
        }

        public static bool HasTiberium(this IntVec3 c, Map map)
        {
            return map.Tiberium().TiberiumInfo.HasTiberiumAt(c);
        }

        public static bool CanGrowFrom(this IntVec3 c, Map map)
        {
            return map.Tiberium().TiberiumInfo.CanGrowFrom(c);
        }

        public static bool CanGrowTo(this IntVec3 c, Map map)
        {
            return map.Tiberium().TiberiumInfo.CanGrowTo(c);
        }

        public static bool IsAffectedCell(this IntVec3 c, Map map)
        {
            return map.Tiberium().TiberiumInfo.IsAffectedCell(c);
        }

        public static bool IsSuppressed(this IntVec3 c, Map map)
        {
            return map.Tiberium().SuppressionInfo.IsSuppressed(c);
        }

        public static bool ShouldGrowFloraAt(IntVec3 c, Map map)
        {
            return map.Tiberium().FloraInfo.ShouldGrowFloraAt(c);
        }
        
        
        //Terrain Checks
        public static bool IsSoil(this TerrainDef def)
        {
            return def.IsSoil || TiberiumDefOf.TerrainFilter_Soil.Allows(def);
        }

        public static bool IsMoss(this TerrainDef def)
        {
            return TiberiumDefOf.TerrainFilter_Moss.Allows(def);
        }

        public static bool IsSand(this TerrainDef def)
        {
            return TiberiumDefOf.TerrainFilter_Sand.Allows(def);
        }

        public static bool IsStone(this TerrainDef def)
        {
            return TiberiumDefOf.TerrainFilter_Stone.Allows(def);
        }
    }
}
