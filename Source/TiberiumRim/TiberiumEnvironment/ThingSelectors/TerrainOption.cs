using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class TerrainOption : DefFloat<TerrainDef>
    {
        public bool isTopLayer;

        public TerrainOption(string def, float value) : base(def, value)
        {
        }

        public TerrainOption(TerrainDef def, float value) : base(def, value)
        {
        }

        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = s.Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(def), array[0], null, null);
            if (array.Length > 1)
                this.value = ParseHelper.ParseFloat(array[1]);
            if (array.Length > 2)
                this.isTopLayer = ParseHelper.ParseBool(array[2]);
        }
    }
}
