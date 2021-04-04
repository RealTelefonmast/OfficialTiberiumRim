using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class ThingOptionsDef : Def
    {
        public List<WeightedThing> options;

        public ThingDef SelectRandomOptionByChance()
        {
            return options.First(t => TRUtils.Chance(t.weight)).thing;
        }

        public ThingDef SelectRandomOptionByWeight()
        {
            return options.RandomElementByWeight(t => t.weight).thing;
        }
    }

    public class ThingOption
    {
        public string defName;

        public ThingOption(string defName)
        {
            this.defName = defName;
        }

        public ThingOptionsDef OptionsDef => DefDatabase<ThingOptionsDef>.GetNamed(defName, false);
        public ThingDef SingleThing => DefDatabase<ThingDef>.GetNamed(defName, false);

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            defName = xmlRoot.FirstChild.Value;
        }

        public ThingDef GetOutcome()
        {
            if (OptionsDef != null)
                return OptionsDef.SelectRandomOptionByWeight();
            return SingleThing;
        }
    }
}
