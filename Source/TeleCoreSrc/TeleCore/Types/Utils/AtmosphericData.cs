using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TeleCore.Types.Utils;

[StaticConstructorOnStartup]
public static class AtmosphericData
{
    public static readonly Dictionary<StuffCategoryDef, float> PassPercentByStuff;

    static AtmosphericData()
    {
        PassPercentByStuff = new Dictionary<StuffCategoryDef, float>
        {
            { StuffCategoryDefOf.Fabric, 0.9f },
            { StuffCategoryDefOf.Leathery, 0.5f },
            { StuffCategoryDefOf.Woody, 0.25f },
            { StuffCategoryDefOf.Stony, 0.0625f },
            { StuffCategoryDefOf.Metallic, 0f }
        };
    }
}