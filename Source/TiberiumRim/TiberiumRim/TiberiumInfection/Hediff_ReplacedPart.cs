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
            for (int i = 0; i < base.Part.parts.Count; i++)
            {
                Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this.pawn, null);
                hediff_MissingPart.IsFresh = false;
                hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
                hediff_MissingPart.Part = base.Part.parts[i];
                this.pawn.health.hediffSet.AddDirect(hediff_MissingPart, null, null);
            }
        }
    }
}
