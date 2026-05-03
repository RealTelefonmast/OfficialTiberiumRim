using System;
using System.Text.RegularExpressions;
using System.Xml;
using TR.ThingSelectors;
using Verse;
using ThingOption = Verse.ThingOption;

namespace TR.Conversions;

public class ThingConversion
{
    //public List<WeightedThing> toThing;
    public float chance = 1f;
    public FilterOption filter;
    public ThingOption toThing;

    public bool HasOutcomesFor(Thing thing)
    {
        return HasOutcomesFor(thing.def);
    }

    public bool HasOutcomesFor(ThingDef def)
    {
        if (filter.FilterDef != null)
            return filter.FilterDef.Allows(def);
        return filter.SingleThing == def;
    }

    public ThingDef GetOutcome()
    {
        //Log.Message("ToThing? " + (toThing != null) + " |single? " + toThing?.SingleThing + " - " + toThing?.OptionsDef);
        return toThing.GetOutcome();
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var arr1 = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "")
            .Split(new[] { "->" }, StringSplitOptions.None); //Split(',');
        var arr2 = arr1[1].Split(',');

        filter = new FilterOption(arr1[0]);
        toThing = new ThingOption(arr2[0]);
        chance = ParseHelper.ParseFloat(arr2[1]);
    }
}