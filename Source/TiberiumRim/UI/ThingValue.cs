using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using Verse;

namespace TiberiumRim;

public sealed class ThingValue : Editable, IExposable
{
    public float chance = 1f;
    public string defName;

    private PawnKindDef pawnKind;
    /* The ThingValue
     * It allows a detailed description of an object group in a single line
     */

    //TODO: Organize ThingValue - Peer Review 
    public QualityCategory QualityCategory = QualityCategory.Awful;
    private ThingDef stuffDef;
    private ThingDef thingDef;
    public int value = 1;

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

    public bool IsPawnKindDef => DefDatabase<PawnKindDef>.GetNamedSilentFail(defName) != null;

    public ThingDef ResolvedStuff
    {
        get
        {
            if (ThingDef?.MadeFromStuff ?? false) stuffDef ??= GenStuff.DefaultStuffFor(ThingDef);
            return stuffDef;
        }
    }

    public ThingDef ThingDef
    {
        get
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null)
                if (PawnKindDef != null)
                    def = DefDatabase<ThingDef>.GetNamedSilentFail(PawnKindDef.race.defName);
            return def;
        }
    }

    public PawnKindDef PawnKindDef => DefDatabase<PawnKindDef>.GetNamedSilentFail(defName);

    public bool Valid => ThingDef != null || PawnKindDef != null;

    public string Summary => defName + QualityCategory + stuffDef + value + chance;

    public void ExposeData()
    {
        Scribe_Values.Look(ref QualityCategory, "qc");
        Scribe_Values.Look(ref defName, "defName");
        Scribe_Values.Look(ref value, "value");
        Scribe_Values.Look(ref chance, "chance");
    }

    public override IEnumerable<string> ConfigErrors()
    {
        if (ThingDef == null) yield return "Can't find thing or pawn with defName: " + defName;
    }

    public bool ThingFits(Thing thing)
    {
        if (stuffDef != null && thing.Stuff != stuffDef) return false;

        if (!(thing.TryGetQuality(out var qc) && qc < QualityCategory)) return false;
        return true;
    }

    //Notation - <defName>value, chance, quality, stuff</defName>
    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        defName = xmlRoot.Name;
        var values = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "").Split(',');

        var count = values.Length;
        value = (int)ParseHelper.FromString(values[0], typeof(int));
        if (count > 1)
            chance = (float)ParseHelper.FromString(values[1], typeof(float));
        if (count > 2)
            QualityCategory = (QualityCategory)ParseHelper.FromString(values[3], typeof(QualityCategory));
        if (count > 3)
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stuffDef", values[3]);
    }

    public override string ToString()
    {
        var defName = this.defName;
        var quality = QualityCategory.ToString();
        var stuff = stuffDef?.defName;
        if (quality.NullOrEmpty() && stuff.NullOrEmpty()) return defName + "," + value + "," + chance;
        return "(" + defName + "," + QualityCategory + "," + stuff + ")," + value + "," + chance;
    }

    public override int GetHashCode()
    {
        return (ToString().GetHashCode() + value) << 16;
    }
}