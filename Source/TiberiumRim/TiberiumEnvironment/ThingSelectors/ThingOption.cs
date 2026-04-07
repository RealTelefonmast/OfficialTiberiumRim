using System.Xml;
using TR.Defs;
using Verse;

namespace TR.TiberiumEnvironment.ThingSelectors;

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