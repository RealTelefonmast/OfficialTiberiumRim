using System.Xml;
using Verse;

namespace TR
{

    public class FilterOption
    {
        public string defName;

        public FilterOption(string defName)
        {
            this.defName = defName;
        }

        public ThingFilterDef FilterDef => DefDatabase<ThingFilterDef>.GetNamed(defName, false);
        public ThingDef SingleThing => DefDatabase<ThingDef>.GetNamed(defName, false);

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            defName = xmlRoot.FirstChild.Value;
        }
    }
}
