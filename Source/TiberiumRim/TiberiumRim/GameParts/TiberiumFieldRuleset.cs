using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumFieldRuleset
    {
        public List<ThingGroupChance> floraOptions;
        public List<TerrainFloat> terrainOptions;
        public List<WeightedThing> crystalOptions;
        public bool allowFlora = true;

        [Unsaved]
        private float maxWeight;

        public TiberiumCrystalDef RandomTiberiumType()
        {
            return (TiberiumCrystalDef)crystalOptions.RandomElementByWeight(t => t.weight).thing;
        }

        public TRThingDef RandomPlant()
        {
            return (TRThingDef)floraOptions.SelectMany(o => o.plants).RandomElementByWeight(p => p.weight).thing;
        }

        public TRThingDef PlantAt(float distance, float maxDistance)
        {
            //"Chance" in this case is "DistancePercent"
            return (TRThingDef)floraOptions.Where(p => distance >= maxDistance * p.chance).SelectMany(p => p.plants).RandomElementByWeight(p => p.weight).thing;
        }

        public float MaxFloraWeight
        {
            get
            {
                if (maxWeight == 0)
                    maxWeight = floraOptions.Max(t => t.plants.Max(p => p.weight));
                return maxWeight;
            }
        }

        public float ChanceFor(TRThingDef plant, float atDistance, float maxDistance)
        {
            float distanceChance = 1f - Mathf.InverseLerp(0f, maxDistance, atDistance);
            WeightedThing thing = floraOptions.SelectMany(f => f.plants).First(w => w.thing == plant);
            var weightChance = Mathf.InverseLerp(0f, MaxFloraWeight, thing.weight);
            var lerpedChance = Mathf.Lerp(distanceChance, 1f, Mathf.Clamp01(weightChance - (1f - distanceChance)));
            return lerpedChance; //Mathf.Lerp(distanceChance, 1f, Mathf.InverseLerp(0f, MaxFloraWeight, thing?.weight  ?? 0));
        }

        public IEnumerable<TerrainFloat> TerrainOptionsFor(TerrainDef terrain)
        {
            return terrainOptions.Where(t => ((TiberiumTerrainDef)t.terrainDef).SupportsTerrain(terrain));
        }

        public TerrainDef RandomTerrain()
        {
            return terrainOptions.RandomElementByWeight(t => t.value).terrainDef;
        }
    }
}
