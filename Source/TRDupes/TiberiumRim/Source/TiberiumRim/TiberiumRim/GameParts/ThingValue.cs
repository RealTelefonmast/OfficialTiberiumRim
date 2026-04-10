using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public sealed class ThingValue : Editable, IExposable
    {
        /* The ThingValue
         * It allows a detailed description of an object group in a single line
        */

        //TODO: Organize ThingValue - Peer Review 
        public QualityCategory QualityCategory = QualityCategory.Awful;
        public string defName = null;
        public int value = 1;
        public float chance = 1f;

        private PawnKindDef pawnKind;
        private ThingDef thingDef;
        private ThingDef stuffDef;

        public ThingValue()
        {
        }

        public ThingValue(ThingDef thingDef, ThingDef stuffDef = null, QualityCategory quality = QualityCategory.Awful)
        {
             this.thingDef = thingDef;
             this.stuffDef = stuffDef;
             QualityCategory = quality;
        }

        public ThingValue(PawnKindDef pawnKind)
        {
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (ThingDef == null)
            {
                yield return "Can't find thing or pawn with defName: " + defName;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref QualityCategory, "qc");
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref value, "value");
            Scribe_Values.Look(ref chance, "chance");
        }

        public bool IsPawnKindDef => DefDatabase<PawnKindDef>.GetNamedSilentFail(defName) != null;

        public ThingDef ResolvedStuff
        {
            get
            {
                if (ThingDef?.MadeFromStuff ?? false)
                {
                    stuffDef ??= GenStuff.DefaultStuffFor(ThingDef);
                }
                return stuffDef;
            }
        }

        public ThingDef ThingDef
        {
            get
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def == null)
                {
                    if (PawnKindDef != null)
                        def = DefDatabase<ThingDef>.GetNamedSilentFail(PawnKindDef.race.defName);
                }
                return def;
            }
        }

        public PawnKindDef PawnKindDef => DefDatabase<PawnKindDef>.GetNamedSilentFail(defName);

        public bool Valid => ThingDef != null || PawnKindDef != null;

        public bool ThingFits(Thing thing)
        {
            if (stuffDef != null && thing.Stuff != stuffDef)
            {
                return false;
            }

            if (!(thing.TryGetQuality(out QualityCategory qc) && qc < QualityCategory))
            {
                return false;
            }
            return true;
        }

        //Notation - <defName>value, chance, quality, stuff</defName>
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        { 
            defName = xmlRoot.Name;
            string[] values = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "").Split(',');

            int count = values.Length;
            value  = (int)ParseHelper.FromString(values[0], typeof(int));
            if(count > 1)
                chance = (float)ParseHelper.FromString(values[1], typeof(float));
            if (count > 2)
                QualityCategory = (QualityCategory)ParseHelper.FromString(values[3], typeof(QualityCategory));
            if(count > 3)
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stuffDef", values[3]);
        }

        public string Summary => defName + QualityCategory + stuffDef + value + chance;

        public override string ToString()
        {
            string defName = this.defName;
            string quality = QualityCategory.ToString();
            string stuff = stuffDef?.defName;
            if (quality.NullOrEmpty() && stuff.NullOrEmpty())
            {
                return defName + "," + value + "," + chance;
            }
            return "(" + defName + "," + QualityCategory + "," + stuff + ")," + value + "," + chance;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode() + this.value << 16;
        }
    }
}
