using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class WeightedTerrain
    {
        public TerrainDef terrainDef;
        public float weight = 1f;

        public WeightedTerrain() {}

        public WeightedTerrain(TerrainDef terrainDef, float weight)
        {
            this.terrainDef = terrainDef;
            this.weight = weight;
        }

        public virtual void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = s.Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "terrainDef", array[0], null, null);
            if(array.Length > 1)
                this.weight = (float)ParseHelper.FromString(array[1], typeof(float));
        }
    }
}
