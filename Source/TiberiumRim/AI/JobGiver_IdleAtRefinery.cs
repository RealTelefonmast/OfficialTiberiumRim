using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TiberiumRim;

public class JobGiver_IdleAtRefinery : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        TR.Harvester harvester = pawn as TR.Harvester;
        if (harvester.ShouldIdle) return new Job(TiberiumDefOf.IdleAtRefinery, harvester.IdlePos);
        return null;
    }
}

public class JobDriver_IdleAtRefinery : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
        var idle = new Toil();
        idle.initAction = delegate
        {
            TR.Harvester actor = idle.actor as TR.Harvester;
            actor.pather.StopDead();
            actor.Rotation = !actor.MainRefinery.DestroyedOrNull() ? actor.MainRefinery.Rotation.Opposite : Rot4.Random;
        };
        idle.tickAction = delegate
        {
            TR.Harvester actor = idle.actor as TR.Harvester;
            if (actor.ShouldHarvest)
            {
                EndJobWith(JobCondition.InterruptForced);
                return;
            }

            if (!actor.MainRefinery.DestroyedOrNull())
                if (actor.Position != actor.MainRefinery.InteractionCell)
                    EndJobWith(JobCondition.InterruptForced);
        };
        idle.FailOn(() => !((TR.Harvester)pawn).ShouldIdle);
        idle.FailOnDespawnedOrNull(TargetIndex.A);
        idle.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        idle.defaultCompleteMode = ToilCompleteMode.Never;
        yield return idle;
    }
}