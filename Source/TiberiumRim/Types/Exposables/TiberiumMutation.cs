using System.Collections.Generic;
using TeleCore.Unsorted;
using Verse;

namespace TiberiumRim;

public class TiberiumMutation : IExposable
{
    private List<Pair<BodyPartRecord, bool>> affectedBodyParts = new();
    //TODO: Full implementation of detailed mutation progression

    private Hediff_Mutation parentHediff;

    public Pawn Pawn => parentHediff.pawn;

    public void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
        }

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
        }
    }

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
            TRHediffUtils.MutatePart(Pawn, part, TRHediffDefOf.Crystallized);
        //else if (TRUtils.Chance(HediffUtils.HediffCoverageFor(Pawn, part, TRHediffDefOf.SymbioticPart)))
        //TRHediffUtils.MutatePart(Pawn, part, TRHediffDefOf.Enhanced);
        //else
        //TRHediffUtils.MutatePart(Pawn, part, TRHediffDefOf.Visceral);
    }
}