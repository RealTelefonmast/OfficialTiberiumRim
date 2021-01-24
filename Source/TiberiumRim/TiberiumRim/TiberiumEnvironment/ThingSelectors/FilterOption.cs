using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace TiberiumRim
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
