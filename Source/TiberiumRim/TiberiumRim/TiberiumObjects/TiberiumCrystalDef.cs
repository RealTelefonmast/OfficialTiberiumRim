using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine.Windows.WebCam;

namespace TiberiumRim
{
    public class TiberiumCrystalDef : TRThingDef
    {
        public TiberiumCrystalProperties tiberium;

        //Corruptions
        public ThingDef monolith;
        public ThingDef rock;
        public ThingDef wall;
        public ThingDef chunk;

        //Terrain
        public TerrainDef dead;
        public TiberiumConversionRulesetDef conversionRuleset;

        public TiberiumCrystalDef() : base()
        {
        }

        public TerrainDef TerrainFrom(TerrainDef terrain)
        {
            return conversionRuleset.ConversionFor(terrain)?.toTerrainDef;
        }

        public bool HasOutcomesFor(TerrainDef terrain)
        {
            return TerrainFrom(terrain) != null;
        }

        public bool HasOutcomesAt(IntVec3 pos, Map map)
        {
            conversionRuleset.GetOutcomes(pos, map, out TerrainDef topTerrain, out TerrainDef underTerrain, out TiberiumCrystalDef crystalDef);
            return (topTerrain != null || underTerrain != null);
        }

        public void SpreadOutcomesAt(IntVec3 pos, Map map, out TerrainDef topTerrain, out TerrainDef underTerrain, out TiberiumCrystalDef crystalDef)
        {
            conversionRuleset.GetOutcomes(pos, map, out topTerrain, out underTerrain, out crystalDef);
        }

        public bool FloraOutcomesFor(ThingDef plantDef, out TRThingDef toPlant, out TerrainDef plantTerrain)
        {
            toPlant = null;
            plantTerrain = null;
            if (conversionRuleset.floraConversions.NullOrEmpty()) return false;
            var conversion = conversionRuleset.floraConversions.Find(f => f.PlantContained(plantDef));
            if (conversion == null) return false;
            toPlant = (TRThingDef)conversion.toPlantOption.SelectRandomOptionByWeight();
            plantTerrain = conversion.toTerrainOption.SelectRandomOptionByChance();
            return true;
        }

        public TiberiumValueType TiberiumValueType => tiberium.type;

        public HarvestType HarvestType
        {
            get
            {
                return TiberiumValueType switch
                {
                    TiberiumValueType.Unharvestable => HarvestType.Unharvestable,
                    TiberiumValueType.Sludge => HarvestType.Unvaluable,
                    _ => HarvestType.Valuable
                };
            }
        }

        public bool IsInfective => tiberium.infects;
        public bool IsMoss => HarvestType == HarvestType.Unvaluable;
    }
}
