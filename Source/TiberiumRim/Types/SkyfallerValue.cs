using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim;

public class SkyfallerValue
{
    public int amount = 1;

    public float chance = 1f;

    //Skyfaller definition with both inner thing, an amount and a chance
    public ThingDef innerThing;
    public ThingDef skyfallerDef;

    public SkyfallerValue()
    {
    }

    public SkyfallerValue(ThingDef skyfallerDef, ThingDef innerThing, int amount = 1, float chance = 1)
    {
        this.skyfallerDef = skyfallerDef;
        this.innerThing = innerThing;
        this.amount = amount;
        this.chance = chance;
    }

    //Notation <SkyfallerDef>InnerThingDef, amount, chance</SkyfallerDef>
    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skyfallerDef", xmlRoot.Name);
        var values = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "").Split(',');

        var count = values.Length;
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "innerThing", values[0]);
        if (count > 1)
            amount = (int)ParseHelper.FromString(values[1], typeof(int));
        if (count > 2)
            chance = (float)ParseHelper.FromString(values[2], typeof(float));
    }
}