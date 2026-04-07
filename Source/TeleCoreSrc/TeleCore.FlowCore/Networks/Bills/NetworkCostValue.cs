using System.Xml;
using Verse;

namespace TeleCore.FlowCore.Bills;

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