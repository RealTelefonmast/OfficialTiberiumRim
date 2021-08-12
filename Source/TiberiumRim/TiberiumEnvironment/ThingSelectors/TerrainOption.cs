using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class TerrainOption : WeightedTerrain
    {
        public bool isTopLayer;

        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = s.Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "terrainDef", array[0], null, null);
            if (array.Length > 1)
                this.weight = ParseHelper.ParseFloat(array[1]);
            if (array.Length > 2)
                this.isTopLayer = ParseHelper.ParseBool(array[2]);
        }
    }
}
