using System.Collections.Generic;
using TR.Comps;
using Verse;
using Verse.AI;

namespace TR.AI;

public class JobGiver_WanderAtParent : ThinkNode_JobGiver
{
    public override Job TryGiveJob(Pawn pawn)
    {
        var wanderComp = pawn.GetComp<Comp_WanderProps>();
        if (wanderComp == null) return null;
        var gotoIdle = wanderComp.GetRandomCell();
        if (!GenSight.LineOfSight(wanderComp.IPawn.Parent.Position, gotoIdle, wanderComp.parent.Map)) return null;
        if (!wanderComp.IPawn.CanWander) return null;
        var job = DefDatabase<JobDef>.GetNamed("WanderAtParent");
        return JobMaker.MakeJob(job, gotoIdle);
    }
}

public class JobDriver_WanderAtParent : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        var gotoIdle = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
        gotoIdle.FailOn(() => !pawn.CanReach(TargetA, PathEndMode.OnCell, Danger.Deadly));
        gotoIdle.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        yield return gotoIdle;
    }
}