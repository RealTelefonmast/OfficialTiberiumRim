using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class TerrainConversion
    {
        public TerrainFilter filter;
        public List<DefFloat<TerrainDef>> toTerrain;

        public bool Supports(TerrainDef def)
        {
            return filter.Supports(def);
        }

        public TerrainDef RandomOutcome()
        {
            return toTerrain.RandomElementByWeight(w => w.value).def;
        }
    }
}
