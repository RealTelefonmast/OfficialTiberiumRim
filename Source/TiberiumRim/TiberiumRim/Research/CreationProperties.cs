using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class CreationProperties
    {
        public List<CreationOption> thingsToCreate;

        public int TotalCountToMake => thingsToCreate.Sum(t => t.amount);

    }

    public class CreationOption
    {
        public ThingDef def;
        public ThingDef stuffDef;
        public int amount = 1;
        public QualityCategory? quality;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name, null, null);
            this.amount = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
        }
    }
}
