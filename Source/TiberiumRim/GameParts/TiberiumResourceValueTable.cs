using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TR;

public class ResourceValue
{
    public ThingDef resource;
    public DefValue<TiberiumCrystalDef> specificCost;
}

public static class TiberiumResourceValueTable
{
    public static Dictionary<ThingDef, List<Pair<TiberiumValueType, float>>> ResourceValues = new();

    static TiberiumResourceValueTable()
    {
        ResourceValues.Add(ThingDefOf.Steel, new List<Pair<TiberiumValueType, float>>
        {
            new(TiberiumValueType.Green, 4),
            new(TiberiumValueType.Blue, 2),
            new(TiberiumValueType.Red, 1)
        });
        ResourceValues.Add(ThingDefOf.Gold, new List<Pair<TiberiumValueType, float>>
        {
            new(TiberiumValueType.Green, 30),
            new(TiberiumValueType.Blue, 15),
            new(TiberiumValueType.Red, 5)
        });
        ResourceValues.Add(ThingDefOf.Steel, new List<Pair<TiberiumValueType, float>>
        {
            new(TiberiumValueType.Green, 4),
            new(TiberiumValueType.Blue, 2),
            new(TiberiumValueType.Red, 1)
        });
    }
}