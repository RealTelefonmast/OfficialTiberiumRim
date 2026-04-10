using System;
using System.Linq;
using RimWorld;
using TeleCore.Hediffs;
using Verse;
using Verse.AI;

namespace TeleCore.Utils;

public static class HediffRangedUtility
{
    public static void CheckForAutoAttack(JobDriver driver)
    {
        if (driver.pawn.WorkTagIsDisabled(WorkTags.Violent)) return;

        var hediffs = driver.pawn.health.hediffSet.hediffs;
        foreach (var hediff in hediffs)
        {
            var rangedHediff = hediff.TryGetComp<HediffComp_RangedVerb>();
            if (rangedHediff == null || !rangedHediff.CanAttack) return;
            var verbs = rangedHediff.AllVerbs;
            foreach (var verb in verbs)
            {
                if (verb.IsMeleeAttack) continue;
                var targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns |
                                      TargetScanFlags.NeedThreat;
                if (verb.IsIncendiary())
                    targetScanFlags |= TargetScanFlags.NeedNonBurning;

                var bestTarget =
                    (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(driver.pawn, targetScanFlags);
                if (bestTarget == null) continue;
                //rangedHediff;
                verb.verbProps.warmupTime = 0;
                verb.TryStartCastOn(bestTarget);
            }

            rangedHediff.CanAttack = false;
        }
    }

    public static bool PawnHasRangedHediffVerb(this Pawn pawn)
    {
        return pawn.health.hediffSet.GetAllComps().Any(c => c is HediffComp_RangedVerb);
    }

    public static Action GetRangedAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
    {
        failStr = "";
        var primaryVerb = pawn.BestHediffVerbFor();
        if (primaryVerb == null || primaryVerb.IsMeleeAttack) return null;
        if (!pawn.Drafted)
        {
            failStr = "IsNotDraftedLower".Translate(pawn.LabelShort, pawn);
        }
        else if (!pawn.IsColonistPlayerControlled)
        {
            failStr = "CannotOrderNonControlledLower".Translate();
        }
        else if (target.IsValid && !primaryVerb.CanHitTarget(target))
        {
            if (!pawn.Position.InHorDistOf(target.Cell, primaryVerb.verbProps.range))
                failStr = "OutOfRange".Translate();
            var num = primaryVerb.verbProps.EffectiveMinRange(target, pawn);
            if (pawn.Position.DistanceToSquared(target.Cell) < num * num)
                failStr = "TooClose".Translate();
            else
                failStr = "CannotHitTarget".Translate();
        }
        else if (pawn.WorkTagIsDisabled(WorkTags.Violent))
        {
            failStr = "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn);
        }
        else if (pawn == target.Thing)
        {
            failStr = "CannotAttackSelf".Translate();
        }
        else
        {
            Pawn target2;
            if ((target2 = target.Thing as Pawn) == null ||
                (!pawn.InSameExtraFaction(target2, ExtraFactionType.HomeFaction) &&
                 !pawn.InSameExtraFaction(target2, ExtraFactionType.MiniFaction)))
                return delegate
                {
                    var job = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                };
            failStr = "CannotAttackSameFactionMember".Translate();
        }

        failStr = failStr.CapitalizeFirst();
        return null;
    }

    public static Verb BestHediffVerbFor(this Pawn pawn)
    {
        foreach (var hediff in pawn.health.hediffSet.hediffs)
        {
            var rangedComp = hediff.TryGetComp<HediffComp_RangedVerb>();
            if (rangedComp == null) continue;
            foreach (var verb in rangedComp.AllVerbs)
            {
                if (verb.WarmingUp) continue;
                return verb;
            }
        }

        return null;
    }
}