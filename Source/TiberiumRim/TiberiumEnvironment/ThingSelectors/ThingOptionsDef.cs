using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class ThingOptionsDef : Def
    {
        public List<DefFloat<ThingDef>> options;

        public ThingDef SelectRandomOptionByChance()
        {
            return options.First(t => TRandom.Chance(t.value)).def;
        }

        public ThingDef SelectRandomOptionByWeight()
        {
            return options.RandomElementByWeight(t => t.value).def;
        }
    }
}
