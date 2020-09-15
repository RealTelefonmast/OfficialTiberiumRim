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
        public List<WeightedThing> crystalOptions;
        public List<TerrainConversion> terrainRules;
        public bool allowFlora = true;
        public bool createBlossom;

        [Unsaved]
        private float maxWeight;

        public float MaxFloraWeight
        {
            get
            {
                if (maxWeight <= 0)
                    maxWeight = floraOptions.Max(t => t.things.Max(p => p.weight));
                return maxWeight;
            }
        }

        public TiberiumCrystalDef RandomTiberiumType()
        {
            return (TiberiumCrystalDef)crystalOptions.RandomElementByWeight(t => t.weight).thing;
        }

        public TRThingDef RandomPlant()
        {
            return (TRThingDef)floraOptions.SelectMany(o => o.things).RandomElementByWeight(p => p.weight).thing;
        }

        public TRThingDef PlantAt(float distance, float maxDistance)
        {
            //"Chance" in this case is "DistancePercent"
            return (TRThingDef)floraOptions.Where(p => distance >= maxDistance * p.chance).SelectMany(p => p.things).RandomElementByWeight(p => p.weight).thing;
        }

        public float ChanceFor(TRThingDef plant, float atDistance, float maxDistance)
        {
            float distanceChance = 1f - Mathf.InverseLerp(0f, maxDistance, atDistance);
            WeightedThing thing = floraOptions.SelectMany(f => f.things).First(w => w.thing == plant);
            var weightChance = Mathf.InverseLerp(0f, MaxFloraWeight, thing.weight);
            var lerpedChance = Mathf.Lerp(distanceChance, 1f, Mathf.Clamp01(weightChance - (1f - distanceChance)));
            return lerpedChance; //Mathf.Lerp(distanceChance, 1f, Mathf.InverseLerp(0f, MaxFloraWeight, thing?.weight  ?? 0));
        }

        public List<WeightedTerrain> TerrainOutcomes(TerrainDef terrain)
        {
            return terrainRules.Find(t => t.Supports(terrain)).toTerrain;
        }

        public TerrainConversion TerrainConversionFor(TerrainDef terrain)
        {
            return terrainRules.Find(t => t.Supports(terrain));
        }

        public TerrainDef RandomOutcome(TerrainDef terrain)
        {
            return TerrainConversionFor(terrain)?.RandomOutcome();
        }
    }
}
