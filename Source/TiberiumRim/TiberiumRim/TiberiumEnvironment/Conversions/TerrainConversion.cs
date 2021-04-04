using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class TerrainConversion
    {
        public TerrainFilter filter;
        public List<WeightedTerrain> toTerrain;

        public bool Supports(TerrainDef def)
        {
            return filter.Supports(def);
        }

        public TerrainDef RandomOutcome()
        {
            return toTerrain.RandomElementByWeight(w => w.weight).terrainDef;
        }
    }
}
