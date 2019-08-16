using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Hediff_MutationPart : Hediff
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (Part == null)
            {
                Log.Message("Bodypart is null for mutation part on " + pawn.LabelShort);
                pawn.health.RemoveHediff(this);
                return;
            }
            CreatePotentialMutation();
        }

        private void CreatePotentialMutation()
        {
            if (TRUtils.Chance(HediffUtils.HediffCoverageFor(pawn, Part, TRHediffDefOf.TiberiumCrystallization)))
                HediffUtils.MutatePart(pawn, Part, TRHediffDefOf.Crystallized);
            else if (TRUtils.Chance(HediffUtils.HediffCoverageFor(pawn, Part, TRHediffDefOf.SymbioticPart)))
                HediffUtils.MutatePart(pawn, Part, TRHediffDefOf.Enhanced);
            else
                HediffUtils.MutatePart(pawn, Part, TRHediffDefOf.Visceral);
        }
    }
}
