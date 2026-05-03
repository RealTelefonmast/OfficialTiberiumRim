using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim;

public class TerrainTagDef : Def
{
    public List<string> extraTags = new();
    public List<string> ignoreTags = new();
    public List<string> tags = new();

    public bool SupportsDef(TerrainDef def)
    {
        var defName = def.defName.ToLower();
        if (def.tags != null)
            return def.tags.Any(t => tags.Contains(t.ToLower()));
        if (!tags.Any(i => defName.Contains(i)))
            return false;
        if (!extraTags.NullOrEmpty() && !extraTags.Any(i => defName.Contains(i)))
            return false;
        if (!ignoreTags.NullOrEmpty() && ignoreTags.Any(i => defName.Contains(i)))
            return false;
        return true;
    }
}

public class TerrainSupport
{
    public TiberiumCrystalDef CrystalOutcome;
    public TiberiumTerrainDef TerrainOutcome;
    public TerrainTagDef TerrainTag;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "TerrainTag", xmlRoot.Name);
        var parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "TerrainOutcome", parts[0]);
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "CrystalOutcome", parts[1]);
    }
}