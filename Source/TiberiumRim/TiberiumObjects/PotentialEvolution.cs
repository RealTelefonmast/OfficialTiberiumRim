using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TR;

public class PotentialEvolution
{
    public float chance;
    public int days = 1;
    public TiberiumProducerDef evolvedDef;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        if (xmlRoot.Name == "li") return;
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "evolvedDef", xmlRoot.Name);
        var parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
        chance = ParseHelper.FromString<float>(parts[0]);
        if (parts.Length == 3)
            days = ParseHelper.FromString<int>(parts[1]);
    }
}