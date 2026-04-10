using RimWorld;
using Verse;

namespace TR.TiberiumInfection;

public class Hediff_ReplacedPart : Hediff_AddedPart
{
    public override void PostAdd(DamageInfo? dinfo)
    {
        pawn.health.RestorePart(Part, this, false);
        foreach (var part in Part.parts)
        {
            var hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
            hediff_MissingPart.IsFresh = false;
            hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
            hediff_MissingPart.Part = part;
            pawn.health.hediffSet.AddDirect(hediff_MissingPart);
        }
    }
}