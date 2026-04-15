using System;
using Verse;
using System.Xml;
using RimWorld;

namespace TiberiumRim
{
    public sealed class SkillRequirement
    {
        public SkillDef skillDef;
        public int skillLevel;

        public SkillRequirement()
        {
        }

        public SkillRequirement(SkillDef def, int level)
        {
            this.skillDef = def;
            this.skillLevel = level;
        }

        public string Summary
        {
            get
            {
                return this.skillLevel + "x " + ((this.skillDef == null) ? "null" : this.skillDef.label);
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                Log.Error("Missconfigured skill requirement: " + xmlRoot.OuterXml);
                return;
            }
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skillDef", xmlRoot.Name);
            this.skillLevel = (int)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(int));
            if(skillLevel > 20)
            {
                Log.Error("Skill level higher than 20: " + xmlRoot.InnerXml);
            }
        }

        public override string ToString()
        {
            return string.Concat(new object[]
            {
                "(",
                this.skillLevel,
                "x ",
                (this.skillDef == null) ? "null" : this.skillDef.defName,
                ")"
            });
        }

        public override int GetHashCode()
        {
            return (int)this.skillDef.shortHash + this.skillLevel << 16;
        }
    }
}
