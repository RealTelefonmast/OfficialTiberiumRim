using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class TerrainOptionsDef : Def
    {
        public List<WeightedTerrain> options;

        public TerrainDef SelectRandomOptionByChance()
        {
            return options.First(t => TRUtils.Chance(t.weight)).terrainDef;
        }

        public TerrainDef SelectRandomOptionByWeight()
        {
            return options.RandomElementByWeight(t => t.weight).terrainDef;
        }
    }
}
