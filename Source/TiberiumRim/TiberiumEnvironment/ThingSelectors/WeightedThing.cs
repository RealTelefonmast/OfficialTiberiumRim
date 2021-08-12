using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class WeightedThing
    {
        public ThingDef thing;
        public float weight = 1;

        public WeightedThing(){}

        public WeightedThing(string thingDef, float weight)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", thingDef);
            this.weight = weight;
        }

        public WeightedThing(ThingDef thingDef, float weight)
        {
            this.thing = thingDef;
            this.weight = weight;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string[] parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", parts[0]);
            if (parts.Length > 1)
                weight = ParseHelper.FromString<float>(parts[1]);
        }
    }
}
