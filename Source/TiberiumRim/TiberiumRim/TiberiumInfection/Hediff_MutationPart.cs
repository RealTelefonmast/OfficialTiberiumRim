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
                pawn.health.RemoveHediff(this);
                return;
            }

            CreatePotentialMutation();
        }

        private void CreatePotentialMutation()
        {
            if (TRUtils.Chance(HediffCoverageFor(Part, TRHediffDefOf.TiberiumCrystallization)))
                HediffUtils.MutatePart(pawn, Part, TRHediffDefOf.Crystallized);
            else if (TRUtils.Chance(HediffCoverageFor(Part, TRHediffDefOf.SymbioticPart)))
                HediffUtils.MutatePart(pawn, Part, TRHediffDefOf.Enhanced);
            else
                HediffUtils.MutatePart(pawn, Part, TRHediffDefOf.Visceral);
        }

        private float HediffCoverageFor(BodyPartRecord part, HediffDef coverageOf)
        {
            float num = 1;
            float parts = part.parts.Count + 1;
            IEnumerable<Hediff> hediffs = pawn.health.hediffSet.hediffs.Where(h => h.def == coverageOf);
            if (!part.parts.NullOrEmpty())
                foreach (BodyPartRecord potPart in part.parts)
                {
                    if (hediffs.Any(h => h.Part == potPart))
                    {
                        num++;
                    }
                }
            return num / parts;
        }
    }
}
