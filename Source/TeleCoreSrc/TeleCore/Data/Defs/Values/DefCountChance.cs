using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TeleCore.Defs.Values;

public class DefCountChance
{
    public float chance;
    public int count;
    public Def def;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var innerValue = xmlRoot.InnerText;
        var s = Regex.Replace(innerValue, @"\s+", "");
        var array = s.Split(',');
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, $"{nameof(def)}", array[0]);
        if (array.Length > 1)
            count = ParseHelper.FromString<int>(array.Length > 1 ? array[1] : "1");
        if (array.Length > 2)
            chance = ParseHelper.FromString<float>(array.Length > 1 ? array[2] : "1");
    }
}