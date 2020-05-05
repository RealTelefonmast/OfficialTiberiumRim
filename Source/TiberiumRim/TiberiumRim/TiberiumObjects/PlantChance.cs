using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class ThingFilterDef : Def
    {
        public ThingFilter filter;

        public bool Allows(ThingDef thing)
        {
            return filter.Allows(thing);
        }
    }

    public class ThingOptionDef : Def
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

    public class ThingGroupChance
    {
        public List<WeightedThing> plants = new List<WeightedThing>();
        public float chance = 1f;
    }

    public class WeightedThing
    {
        public ThingDef thing;
        public float weight = 1;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string[] parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", parts[0]);
            if(parts.Length > 1)
                weight = ParseHelper.FromString<float>(parts[1]);
        }
    }
}
