using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TR.TiberiumEnvironment.ThingSelectors;

public class TerrainOption : WeightedTerrain
{
    public bool isTopLayer;

    public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
        var array = s.Split(',');
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "terrainDef", array[0]);
        if (array.Length > 1)
            weight = ParseHelper.ParseFloat(array[1]);
        if (array.Length > 2)
            isTopLayer = ParseHelper.ParseBool(array[2]);
    }
}