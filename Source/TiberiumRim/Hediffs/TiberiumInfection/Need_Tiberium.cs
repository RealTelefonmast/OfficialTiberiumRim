using System.Collections.Generic;
using RimWorld;
using TR.DefOf;
using TR.Util;
using Verse;

namespace TR.Hediffs.TiberiumInfection;

public class Need_Tiberium : Need
{
    public enum TiberiumNeedCategory
    {
        Statisfied,
        Lacking,
        Urgent
    }

    public Need_Tiberium(Pawn pawn) : base(pawn)
    {
        threshPercents = new List<float>
        {
            0.15f, // Urgent
            0.50f // Lacking
        };
    }

    public override int GUIChangeArrow => IsBeingSatisfied ? 1 : -1;

    public TiberiumNeedCategory CurCategory
    {
        get
        {
            if (CurLevel <= 0.15f) return TiberiumNeedCategory.Urgent;
            if (CurLevel < 0.50f) return TiberiumNeedCategory.Lacking;
            return TiberiumNeedCategory.Statisfied;
        }
    }

    private float TiberiumNeedFallPerTick => def.fallPerDay / 60000f;

    private bool IsBeingSatisfied => IsInTiberium || HasTiberAdd;

    public bool HasTiberAdd => pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberAddHediff);

    private bool IsInTiberium
    {
        get
        {
            if (pawn.Spawned)
                return pawn.Position.GetTiberium(pawn.Map) != null;
            return false;
        }
    }

    public override void SetInitialLevel()
    {
        CurLevelPercentage = Rand.Range(0.2f, 0.5f);
    }

    public override void NeedInterval()
    {
        if (pawn.SpawnedOrAnyParentSpawned)
            if (pawn.CarriedBy == null && !IsBeingSatisfied)
                CurLevel -= TiberiumNeedFallPerTick * 350;
    }
}