using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TiberiumMutation : IExposable
    {
        //TODO: Full implementation of detailed mutation progression

        private Hediff_Mutation parentHediff;
        private List<Pair<BodyPartRecord, bool>> affectedBodyParts = new List<Pair<BodyPartRecord, bool>>();

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {

            }
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                
            }
        }

        public Pawn Pawn => parentHediff.pawn;

        public void AddPart(BodyPartRecord part, bool risk)
        {
            if (part == null)
            {
                Log.Message("Bodypart is null for mutation part on " + Pawn.LabelShort);
                return;
            }
            CreatePotentialMutation(part);
        }

        private void CreatePotentialMutation(BodyPartRecord part)
        {
            if (TRUtils.Chance(HediffUtils.HediffCoverageFor(Pawn, part, TRHediffDefOf.TiberiumCrystallization)))
                HediffUtils.MutatePart(Pawn, part, TRHediffDefOf.Crystallized);
            //else if (TRUtils.Chance(HediffUtils.HediffCoverageFor(Pawn, part, TRHediffDefOf.SymbioticPart)))
                //HediffUtils.MutatePart(Pawn, part, TRHediffDefOf.Enhanced);
            //else
                //HediffUtils.MutatePart(Pawn, part, TRHediffDefOf.Visceral);
        }
    }
}
