using System.Text;
using RimWorld;
using Verse;

namespace TR.Hediffs.TiberiumInfection;

public class Hediff_TRAddedPart : Hediff_AddedPart
{
    public TRHediffDef def => (TRHediffDef)base.def;

    public override string TipStringExtra
    {
        get
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.TipStringExtra);
            stringBuilder.AppendLine("Efficiency".Translate() + ": " +
                                     def.addedPartProps.partEfficiency.ToStringPercent());
            return stringBuilder.ToString();
        }
    }

    public override void PostAdd(DamageInfo? dinfo)
    {
        //Base PostAdd() does fixed crap that we dont want, however we need the base.base.PostAdd()
        if (def.disablesNeed != null) pawn.needs.AddOrRemoveNeedsAsAppropriate();
        if (comps != null)
            foreach (var comp in comps)
                comp.CompPostPostAdd(dinfo);

        if (Part == null)
        {
            Log.Error(def.defName + " has null Part. It should be set before PostAdd.", false);
            return;
        }

        pawn.health.RestorePart(Part, this, false);
        foreach (var part in Part.parts)
        {
            var hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
            hediff_MissingPart.IsFresh = !def.isNaturalInsertion;
            hediff_MissingPart.lastInjury = def.isNaturalInsertion ? null : HediffDefOf.SurgicalCut;
            hediff_MissingPart.Part = part;
            pawn.health.hediffSet.AddDirect(hediff_MissingPart);
        }
    }
}