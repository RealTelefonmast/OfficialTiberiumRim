using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class TiberiumConversionRulesetDef : Def
    {
        public TiberiumCrystalDef baseType;
        public List<TiberiumConversion> conversions;
        public List<FloraConversion> floraConversions;

        public void GetOutcomes(IntVec3 pos, Map map, out TerrainDef top, out TerrainDef under, out TiberiumCrystalDef crystalDef)
        {
            under = null;
            pos.GetTerrain(map, out TerrainDef topTerrain, out TerrainDef underTerrain);
            if (topTerrain.IsTiberiumTerrain())
            {
                top = topTerrain;
                crystalDef = baseType;
                return;
            }
            GetOutcomes(topTerrain, out top, out crystalDef);

            if (!topTerrain.layerable) return;
            GetOutcomes(underTerrain, out under, out crystalDef);
        }

        public void GetOutcomes(TerrainDef from, out TerrainDef terrainOutcome, out TiberiumCrystalDef crystalOutcome)
        {
            var conversion = ConversionFor(from);
            terrainOutcome = conversion?.toTerrainDef;
            crystalOutcome = conversion?.toCrystalDef;
        }

        public TiberiumConversion ConversionFor(TerrainDef def)
        {
            return conversions.Find(t => t.TerrainContained(def));
        }
    }
}
