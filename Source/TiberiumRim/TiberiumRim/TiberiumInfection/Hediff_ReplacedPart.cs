using System;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Hediff_ReplacedPart : Hediff_AddedPart
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            this.pawn.health.RestorePart(base.Part, this, false);
            foreach (var part in base.Part.parts)
            {
                Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this.pawn, null);
                hediff_MissingPart.IsFresh = false;
                hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
                hediff_MissingPart.Part = part;
                this.pawn.health.hediffSet.AddDirect(hediff_MissingPart, null, null);
            }
        }
    }
}
