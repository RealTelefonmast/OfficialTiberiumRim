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
        public ThingDef Stuff;
        public QualityCategory QualityCategory = QualityCategory.Normal;
        public string defName = null;
        public int value = 1;
        public float chance = 1f;
        public bool CustomQuality = false;

        public ThingValue()
        {
        }

        public ThingValue(ThingDef thingDef, ThingDef stuffDef = null, QualityCategory quality = QualityCategory.Normal)
        {

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
            Scribe_Defs.Look(ref Stuff, "Stuff");
            Scribe_Values.Look(ref QualityCategory, "qc");
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref value, "value");
            Scribe_Values.Look(ref chance, "chance");
            Scribe_Values.Look(ref CustomQuality, "cq");
        }

        public bool IsPawnKindDef => DefDatabase<PawnKindDef>.GetNamedSilentFail(defName) != null;

        public ThingDef ResolvedStuff
        {
            get
            {
                if (ThingDef?.MadeFromStuff ?? false)
                {
                    return Stuff ?? GenStuff.DefaultStuffFor(ThingDef);
                }
                return null;
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
                    {
                        def = DefDatabase<ThingDef>.GetNamedSilentFail(PawnKindDef.race.defName);
                    }
                }
                return def;
            }
        }

        public PawnKindDef PawnKindDef => DefDatabase<PawnKindDef>.GetNamedSilentFail(defName);

        public bool Valid => ThingDef != null || PawnKindDef != null;

        public bool ThingFits(Thing thing)
        {
            if (Stuff != null && thing.Stuff != Stuff)
            {
                return false;
            }
            if (CustomQuality && !(thing.TryGetQuality(out QualityCategory qc) && qc == QualityCategory))
            {
                return false;
            }
            return true;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string Child = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string ThingValueString = AdjustString(Child, out string ThingConditionString);
            if (ThingConditionString.NullOrEmpty())
            {
                if (ThingValueString.Contains(','))
                {
                    string[] array = ThingValueString.Split(',');
                    defName = array[0];
                    value = (int)ParseHelper.FromString(array[1], typeof(int));
                    if (array.Count() == 3)
                    {
                        chance = (float)ParseHelper.FromString(array[2], typeof(float));
                    }
                    return;
                }
                defName = ThingValueString;
            }
            else
            {
                ReadThingCondition(ThingConditionString, out defName);
                if (!ThingValueString.NullOrEmpty())
                {
                    if (ThingValueString.Contains(','))
                    {
                        string[] array = ThingValueString.Split(',');
                        value = (int)ParseHelper.FromString(array[0], typeof(int));
                        chance = (float)ParseHelper.FromString(array[1], typeof(float));
                        return;
                    }
                    value = (int)ParseHelper.FromString(ThingValueString, typeof(int));
                }
            }
        }

        private string AdjustString(string s, out string condition)
        {
            condition = "";
            if (s.Contains('('))
            {
                int from = s.IndexOf('(') + 1;
                int to = s.IndexOf(')');
                condition = s.Substring(from, to - from);
                if (s.Length > to + 2)
                {
                    s = s.Substring(to + 2);
                }
                return "";
            }
            return s;
        }

        private void ReadThingCondition(string s, out string defName)
        {
            defName = null;
            if (s.Contains(","))
            {
                string[] array = s.Split(',');
                defName = array[0];
                QualityCategory = (QualityCategory)ParseHelper.FromString(array[1], typeof(QualityCategory));
                CustomQuality = true;
                if (s.Count(c => c == ',') == 2)
                {
                    DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "Stuff", array[2]);
                }
                return;
            }
            defName = s;
        }

        public string Summary => defName + QualityCategory + Stuff + value + chance;

        public override string ToString()
        {
            string defName = this.defName;
            string quality = QualityCategory.ToString();
            string stuff = Stuff?.defName;
            if (quality.NullOrEmpty() && stuff.NullOrEmpty())
            {
                return defName + "," + value + "," + chance;
            }
            return "(" + defName + "," + QualityCategory + "," + Stuff + ")," + value + "," + chance;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode() + this.value << 16;
        }
    }
}
