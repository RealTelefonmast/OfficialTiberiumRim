using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class DefFloat<T> where T : Def
    {
        public T def;
        public float value = 1;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = s.Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", array[0], null, null);
            if (array.Length > 1)
                this.value = (float)ParseHelper.FromString(array[1], typeof(float));
        }

        public override string ToString()
        {
            return def.LabelCap + "(" + value + ")";
        }

        public string ToStringPercent()
        {
            return def.LabelCap + "(" + value.ToStringPercent() + ")";
        }
    }
}
