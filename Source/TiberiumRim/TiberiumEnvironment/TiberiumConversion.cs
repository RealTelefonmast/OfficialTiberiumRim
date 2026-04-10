using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TR;

public class TiberiumConversion
{
    public string fromTerrainDefName;
    public TiberiumCrystalDef toCrystalDef;
    public TerrainDef toTerrainDef;

    public TerrainDef FromTerrain => DefDatabase<TerrainDef>.GetNamedSilentFail(fromTerrainDefName);
    public TerrainFilterDef FromTerrainGroup => DefDatabase<TerrainFilterDef>.GetNamedSilentFail(fromTerrainDefName);

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var array = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "").Split(',');
        fromTerrainDefName = array[0];
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "toTerrainDef", array[1]);
        if (array.Length > 2)
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "toCrystalDef", array[2]);
    }

    public bool TerrainContained(TerrainDef def)
    {
        if (FromTerrain != null)
            return FromTerrain == def;

        return FromTerrainGroup.AllowsTerrainDef(def);
    }
}