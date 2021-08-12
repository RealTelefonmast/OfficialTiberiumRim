using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class PlantConversion
    {
        public FilterOption filter;
        public ThingOptionsDef ToPlantOptions;
        public TerrainOptionsDef ToTerrainOptions;

        public bool HasOutcomesFor(ThingDef def)
        {
            if (filter.FilterDef != null)
                return filter.FilterDef.Allows(def);
            return filter.SingleThing == def;
        }

        public void GetOutcomes( out ThingDef outPlant, out TerrainDef outTerrain)
        {
            outPlant = GetRandomPlant();
            outTerrain = GetRandomTerrain();
        }

        public ThingDef GetRandomPlant()
        {
            return ToPlantOptions.SelectRandomOptionByWeight();
        }

        public TerrainDef GetRandomTerrain()
        {
            return ToTerrainOptions.SelectRandomOptionByWeight();
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string[] array = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "").Split(',');
            filter = new FilterOption(array[0]);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "ToPlantOptions", array[1], null, null);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "ToTerrainOptions", array[2], null, null);
        }
    }
}
