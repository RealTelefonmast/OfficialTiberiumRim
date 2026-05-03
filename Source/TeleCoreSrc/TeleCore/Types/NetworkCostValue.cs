using System.Xml;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public class NetworkCostValue
{
    public float value;
    public NetworkValueDef valueDef;

    public bool HasValue => value > 0;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        if (xmlRoot.Name == "li")
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "valueDef", xmlRoot.FirstChild.Value);
        }
        else
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "valueDef", xmlRoot.Name);
            value = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
        }
    }
}