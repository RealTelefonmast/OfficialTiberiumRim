using System.Collections.Generic;
using System.Linq;
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
        public float tiberiumDensity = 0.05f;

        public SimpleCurve corruptionCurve = new SimpleCurve(new CurvePoint[2] {new CurvePoint(0, 1), new CurvePoint(1, 0)});
        public float corruptionRadius = 0f;
        public bool createBlossom;

        [Unsaved]
        private float maxWeight;

        public bool SpawnsTib => !crystalOptions.NullOrEmpty();

        public float MaxFloraWeight
        {
            get
            {
                if (maxWeight <= 0)
                    maxWeight = floraOptions.Max(t => t.things.Max(p => p.weight));
                return maxWeight;
            }
        }

        public IEnumerable<TiberiumCrystalDef> TiberiumTypes => crystalOptions?.Select(t => t.thing as TiberiumCrystalDef);

        public bool AllowTerrain(TerrainDef terrain)
        {
            return terrainRules.Any(t => t.Supports(terrain));
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
            //The percentual float of the position along the max radius
            float distanceFloat = Mathf.InverseLerp(0f, maxDistance, atDistance);
            //The chance value depending on distance
            float distanceChance = corruptionCurve.Evaluate(distanceFloat);
            //The thing at that position depending on predefined weight
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
