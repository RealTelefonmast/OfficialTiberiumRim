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
        public List<CreationOptionProperties> thingsToCreate;

        public int TotalCountToMake => thingsToCreate.Sum(t => t.amount);

    }

    public class CreationOptionProperties
    {
        public ThingDef def;
        public ThingDef stuffDef;
        public int amount = 1;
        public QualityCategory? quality;


        public bool Accepts(Thing thing)
        {
            return (quality == null || thing.TryGetQuality(out QualityCategory qc) && qc == quality) && (stuffDef == null || thing.Stuff == stuffDef);
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            //TODO: Add additional reader options
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name, null, null);
            this.amount = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
        }
    }
}
