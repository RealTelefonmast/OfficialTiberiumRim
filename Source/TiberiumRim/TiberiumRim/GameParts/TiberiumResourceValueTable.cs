using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class ResourceValue
    {
        public ThingDef resource;
        public TiberiumTypeCost2 specificCost;

    }

    public static class TiberiumResourceValueTable
    {
        public static Dictionary<ThingDef, List<Pair<TiberiumValueType, float>>> ResourceValues = new Dictionary<ThingDef, List<Pair<TiberiumValueType, float>>>();

        static TiberiumResourceValueTable()
        {
            ResourceValues.Add(ThingDefOf.Steel, new List<Pair<TiberiumValueType, float>>()
            {
                new Pair<TiberiumValueType, float>(TiberiumValueType.Green, 4),
                new Pair<TiberiumValueType, float>(TiberiumValueType.Blue, 2),
                new Pair<TiberiumValueType, float>(TiberiumValueType.Red, 1)
            });
            ResourceValues.Add(ThingDefOf.Gold, new List<Pair<TiberiumValueType, float>>()
            {
                new Pair<TiberiumValueType, float>(TiberiumValueType.Green, 30),
                new Pair<TiberiumValueType, float>(TiberiumValueType.Blue, 15),
                new Pair<TiberiumValueType, float>(TiberiumValueType.Red, 5)
            });
            ResourceValues.Add(ThingDefOf.Steel, new List<Pair<TiberiumValueType, float>>()
            {
                new Pair<TiberiumValueType, float>(TiberiumValueType.Green, 4),
                new Pair<TiberiumValueType, float>(TiberiumValueType.Blue, 2),
                new Pair<TiberiumValueType, float>(TiberiumValueType.Red, 1)
            });
        }
    }
}
