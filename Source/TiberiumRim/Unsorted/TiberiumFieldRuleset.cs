using System.Collections.Generic;
using System.Linq;
using TiberiumRim;
using TR.Conversions;
using TR.ThingSelectors;
using UnityEngine;
using Verse;

namespace TR;

public class TiberiumFieldRuleset
{
    public bool allowFlora = true;

    public SimpleCurve corruptionCurve = new(new CurvePoint[2] { new(0, 1), new(1, 0) });
    public float corruptionRadius = 0f;
    public bool createBlossom;
    public List<DefFloat<ThingDef>> crystalOptions;
    public List<ThingGroupChance> floraOptions;

    [Unsaved] private float maxWeight;

    public List<TerrainConversion> terrainRules;
    public float tiberiumDensity = 0.05f;

    public bool SpawnsTib => !crystalOptions.NullOrEmpty();

    public float MaxFloraWeight
    {
        get
        {
            if (maxWeight <= 0)
                maxWeight = floraOptions.Max(t => t.things.Max(p => p.value));
            return maxWeight;
        }
    }

    public IEnumerable<TiberiumCrystalDef> TiberiumTypes => crystalOptions?.Select(t => t.def as TiberiumCrystalDef);

    public bool AllowTerrain(TerrainDef terrain)
    {
        return Enumerable.Any(terrainRules, t => t.Supports(terrain));
    }

    public TiberiumCrystalDef RandomTiberiumType()
    {
        return (TiberiumCrystalDef)crystalOptions.RandomElementByWeight(t => t.value).def;
    }

    public TRThingDef RandomPlant()
    {
        return (TRThingDef)floraOptions.SelectMany(o => o.things).RandomElementByWeight(p => p.value).def;
    }

    public TRThingDef PlantAt(float distance, float maxDistance)
    {
        //"Chance" in this case is "DistancePercent"
        return (TRThingDef)floraOptions.Where(p => distance >= maxDistance * p.chance).SelectMany(p => p.things)
            .RandomElementByWeight(p => p.value).def;
    }

    public float ChanceFor(TRThingDef plant, float atDistance, float maxDistance)
    {
        //The percentual float of the position along the max radius
        var distanceFloat = Mathf.InverseLerp(0f, maxDistance, atDistance);
        //The chance value depending on distance
        var distanceChance = corruptionCurve.Evaluate(distanceFloat);
        //The thing at that position depending on predefined weight
        var thing = floraOptions.SelectMany(f => f.things).First(w => w.def == plant);
        var weightChance = Mathf.InverseLerp(0f, MaxFloraWeight, thing.value);
        var lerpedChance = Mathf.Lerp(distanceChance, 1f, Mathf.Clamp01(weightChance - (1f - distanceChance)));
        return
            lerpedChance; //Mathf.Lerp(distanceChance, 1f, Mathf.InverseLerp(0f, MaxFloraWeight, thing?.weight  ?? 0));
    }

    public List<DefValue<TerrainDef>> TerrainOutcomes(TerrainDef terrain)
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