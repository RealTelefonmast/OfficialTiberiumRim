using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TerrainTagDef : Def
    {
        public List<string> tags = new List<string>();
        public List<string> ignoreTags = new List<string>();
        public List<string> extraTags = new List<string>();

        public bool SupportsDef(TerrainDef def)
        {
            string defName = def.defName.ToLower();
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
        public TerrainTagDef TerrainTag;
        public TiberiumTerrainDef TerrainOutcome;
        public TiberiumCrystalDef CrystalOutcome;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "TerrainTag", xmlRoot.Name);
            string[] parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "TerrainOutcome", parts[0]);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "CrystalOutcome", parts[1]);
        }
    }
}
