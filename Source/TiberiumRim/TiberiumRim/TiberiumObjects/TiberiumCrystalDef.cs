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
        [Unsaved()]
        private float? growthPerTick;
        [Unsaved()]
        private IEnumerable<IntVec3> radialCells;

        public TiberiumCrystalProperties tiberium;
        public TiberiumConversionRulesetDef conversions;

        //Terrain
        public TerrainDef dead;

        public float GrowthPerTick => growthPerTick ??= 1f / ((GenDate.TicksPerDay * tiberium.growDays) / GenTicks.TickLongInterval);

        public IEnumerable<IntVec3> SpreadRangeMask => radialCells ??= GenRadial.RadialPatternInRadius(tiberium.spreadRadius);


        public bool HasOutcomesFor(TerrainDef terrain)
        {
            return conversions.HasOutcomeFor(terrain, out _);
        }

        public bool HasOutcomesAt(IntVec3 pos, Map map)
        {
            return conversions.HasOutcomeFor(pos.GetTerrain(map), out _);
        }

        public bool TryGetTiberiumOutcomesAt(IntVec3 pos, Map map, out TerrainDef topTerrain, out TerrainDef underTerrain, out TiberiumCrystalDef crystalDef)
        {
            conversions.GetOutcomes(pos, map, out topTerrain, out underTerrain, out crystalDef);
            return crystalDef != null;
        }

        public void GetTiberiumOutcomesAt(IntVec3 pos, Map map, out TerrainDef topTerrain, out TerrainDef underTerrain, out TiberiumCrystalDef crystalDef)
        {
            conversions.GetOutcomes(pos, map, out topTerrain, out underTerrain, out crystalDef);
        }

        public bool GetFloraOutcomes(ThingDef plantDef, out ThingDef toPlant, out TerrainDef plantTerrain)
        {
            conversions.GetPlantOutcomes(plantDef, out toPlant, out plantTerrain);
            return toPlant != null;
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

        public bool DamagesThings => tiberium.deteriorationDamage.Average > 0;
    }
}
