using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class TiberiumConversionRulesetDef : Def
    {
        public TiberiumCrystalDef baseType;
        public TiberiumTerrainDef baseTerrain;
        public List<TiberiumConversion> conversions;
        public List<PlantConversion> floraConversions;
        public List<ThingConversion> thingConversions;

        public override IEnumerable<string> ConfigErrors()
        {
            if (baseType == null)
                yield return "Base Type for ConversionRuleset is null!";
        }

        public bool HasOutcomeFor(Thing thing, out ThingConversion conversion)
        {
            conversion = ConversionFor(thing);
            return conversion != null;
        }

        public bool HasOutcomeFor(ThingDef thing, out PlantConversion conversion)
        {
            conversion = ConversionFor(thing);
            return conversion != null;
        }

        public bool HasOutcomeFor(TerrainDef thing, out TiberiumConversion conversion)
        {
            conversion = ConversionFor(thing);
            return conversion != null;
        }

        public TiberiumConversion ConversionForStone()
        {
            return conversions?.First(t => t.FromTerrainGroup == TiberiumDefOf.TerrainFilter_Stone);
        }

        public TiberiumConversion ConversionForSoil()
        {
            return conversions?.First(t => t.FromTerrainGroup == TiberiumDefOf.TerrainFilter_Soil);
        }

        public TiberiumConversion ConversionForSand()
        {
            return conversions?.First(t => t.FromTerrainGroup == TiberiumDefOf.TerrainFilter_Sand);
        }

        public TiberiumConversion ConversionFor(TerrainDef def)
        {
            return conversions?.Find(t => t?.HasOutcomesFor(def) ?? false);
        }

        public PlantConversion ConversionFor(ThingDef def)
        {
            return floraConversions?.Find(t => t?.HasOutcomesFor(def) ?? false);
        }

        public ThingConversion ConversionFor(Thing thing)
        {
            return thingConversions?.Find(t => t?.HasOutcomesFor(thing) ?? false);
        }

        public void GetOutcomes(IntVec3 pos, Map map, out TerrainDef top, out TerrainDef under, out TiberiumCrystalDef crystalDef)
        {
            under = null;
            pos.GetTerrain(map, out TerrainDef topTerrain, out TerrainDef underTerrain);

            GetOutcomes(topTerrain, out top, out crystalDef, out bool isTop);
            if (topTerrain.IsTiberiumTerrain())
                top = topTerrain;
            if (isTop)
                under = topTerrain;

            if (underTerrain == null) return;
            GetOutcomes(underTerrain, out under, out crystalDef, out _);
        }

        public void GetOutcomes(TerrainDef from, out TerrainDef terrainOutcome, out TiberiumCrystalDef crystalOutcome, out bool isTopLayer)
        {
            terrainOutcome = null;
            crystalOutcome = null;
            isTopLayer = false;
            ConversionFor(from)?.GetOutcomes(out crystalOutcome, out terrainOutcome, out isTopLayer);
        }

        public void GetPlantOutcomes(ThingDef inPlant, out ThingDef outPlant, out TerrainDef outTerrain)
        {
            outPlant = null;
            outTerrain = null;
            ConversionFor(inPlant)?.GetOutcomes(out outPlant, out outTerrain);
        }

        public void GetThingOutcomes(Thing thing, out ThingDef thingDef)
        {
            thingDef = ConversionFor(thing)?.GetOutcome();
        }
    }
}
