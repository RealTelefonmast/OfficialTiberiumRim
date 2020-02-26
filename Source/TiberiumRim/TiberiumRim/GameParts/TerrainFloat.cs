using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class TerrainFloat
    {
        public TerrainDef terrainDef;
        public float value = 1f;

        public TerrainFloat()
        {
        }

        public TerrainFloat(TerrainDef terrainDef, float value)
        {
            this.terrainDef = terrainDef;
            this.value = value;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = s.Split(new char[]
            {
                ','
            });
            this.terrainDef = DefDatabase<TerrainDef>.GetNamed(array[0]);
            this.value = (float)ParseHelper.FromString(array[1], typeof(float));
        }
    }
}
