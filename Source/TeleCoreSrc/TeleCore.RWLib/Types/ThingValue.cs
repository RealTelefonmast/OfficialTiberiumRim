using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using Verse;
// ReSharper disable HeapView.BoxingAllocation

namespace TeleCore.RWLib;

/// <summary>
///     Define a Def with various values for dynamic and versatile uses.
///     XML Notation: <defName>value, chance, QualityCategory, stuffDef</defName>
/// </summary>
public sealed class ThingValue : Editable, IExposable
{
    public float chance = 1f;
    public string defName;

    private PawnKindDef pawnKind;
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

    public string Summary => $"{defName} {QualityCategory} {stuffDef} {value} {chance}";

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
            if (thingDef == null)
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def != null)
                    thingDef = def;
                else if (PawnKindDef != null)
                    thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(PawnKindDef.race.defName);
            }

            return thingDef;
        }
    }

    public PawnKindDef PawnKindDef => pawnKind ??= DefDatabase<PawnKindDef>.GetNamedSilentFail(defName);

    public bool IsPawnKindDef => DefDatabase<PawnKindDef>.GetNamedSilentFail(defName) != null;
    public bool Valid => ThingDef != null || PawnKindDef != null;

    public void ExposeData()
    {
        Scribe_Values.Look(ref QualityCategory, "qc");
        Scribe_Values.Look(ref defName, "defName");
        Scribe_Values.Look(ref value, "weight");
        Scribe_Values.Look(ref chance, "chance");
    }

    public override IEnumerable<string> ConfigErrors()
    {
        if (ThingDef == null) yield return $"Can't find thing or pawn with defName: {defName}";
    }

    public bool ThingFits(Thing thing)
    {
        if (thing is Pawn p && p.kindDef != PawnKindDef) return false;
        if (thing.def != ThingDef) return false;
        if (stuffDef != null && thing.Stuff != stuffDef)
            return false;

        return thing.TryGetQuality(out var qc) && qc < QualityCategory;
    }

    //Notation - <defName>weight, chance, quality, stuff</defName>
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
        //TODO: Boxing allocation (string caching?)
        var quality = QualityCategory.ToString();
        var stuff = stuffDef?.defName;
        if (quality.NullOrEmpty() && stuff.NullOrEmpty()) return $"{defName},{value},{chance}";
        return $"({defName},{QualityCategory},{stuff}),{value},{chance}";
    }

    public override int GetHashCode()
    {
        return (ToString().GetHashCode() + value) << 16;
    }
}