using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumProducerDef : TRThingDef
    {
        public ThingDef killedVersion;
        public ThingDef skyfaller;
        public List<TiberiumTerrainDef> tiberiumTerrain = new List<TiberiumTerrainDef>();
        public List<TiberiumCrystalDef> tiberiumTypes = new List<TiberiumCrystalDef>();
        public List<PlantGroupChance> plantsByDistance;
        public List<PotentialEvolution> evolutions;
        public SpawnProperties spawner = new SpawnProperties();
        public float daysToMature = 0f;
        public bool forResearch = true;
        public bool growsFlora = true;
        public bool leaveTiberium = true;

        public ThingDef SelectPlantByDistance(float distance, float maxDistance)
        {
            var list = new List<ThingDef>();
            return plantsByDistance.Where(p => distance >= maxDistance * p.chance).SelectMany(p => p.plants).InRandomOrder().RandomElementByWeight(p => p.weight).plant;
            /*
            foreach (var pgc in plantsByDistance)
            {
                if (distance < maxDistance * pgc.chance) continue;
                foreach(var wp in pgc.plants)
                    if(TRUtils.Chance(wp.weight))
                        list.Add(wp.plant);
            }
            return list.RandomElement();
            */
        }
    }

    public class SpawnProperties
    {
        public TiberiumSpawnMode spawnMode = TiberiumSpawnMode.Direct;
        public IntRange spawnInterval = new IntRange(2500, 5000);
        public IntRange explosionRange = new IntRange(10, 100);
        public FloatRange spreadRange = new FloatRange(-1, -1);
        public IntVec3 sporeOffset = new IntVec3(0, 0, 0);
        public float minDaysToSpread = 0f;
        public float sporeExplosionRadius = 20f;
        public float growRadius = 5f;
    }

    public class PlantGroupProperties
    {
        public IntRange sizeRange = new IntRange(5,10);
        public int minFieldSize = 1000;

        public List<PlantChance> plants = new List<PlantChance>();
    }
}
