using System.Collections.Generic;
using System.Linq;
using TR.GameParts;
using TR.Util;
using Verse;

namespace TR.Defs;

public class ThingOptionsDef : Def
{
    public List<DefFloat<ThingDef>> options;

    public ThingDef SelectRandomOptionByChance()
    {
        return options.First(t => TRUtils.Chance(t.value)).def;
    }

    public ThingDef SelectRandomOptionByWeight()
    {
        return options.RandomElementByWeight(t => t.value).def;
    }
}