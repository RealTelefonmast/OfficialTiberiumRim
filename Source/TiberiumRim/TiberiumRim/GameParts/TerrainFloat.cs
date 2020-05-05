using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class TerrainFloat
    {
        public TerrainDef terrainDef;
        public float value = 1f;

        public TerrainFloat() {}

        public TerrainFloat(TerrainDef terrainDef, float value)
        {
            this.terrainDef = terrainDef;
            this.value = value;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = s.Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "terrainDef", array[0], null, null);
            if(array.Length > 1)
                this.value = (float)ParseHelper.FromString(array[1], typeof(float));
        }
    }
}
