using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class TerrainOptionsDef : Def
    {
        public List<DefFloat<TerrainDef>> options;

        public TerrainDef SelectRandomOptionByChance()
        {
            return options.First(t => TRUtils.Chance(t.value)).def;
        }

        public TerrainDef SelectRandomOptionByWeight()
        {
            return options.RandomElementByWeight(t => t.value).def;
        }
    }
}
