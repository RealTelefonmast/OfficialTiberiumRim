using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobGiver_WanderAtParent : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Comp_WanderProps wanderComp = pawn.GetComp<Comp_WanderProps>();
            if (wanderComp == null) return null;
            IntVec3 gotoIdle = wanderComp.GetRandomCell();
            if (!GenSight.LineOfSight(wanderComp.IPawn.Parent.Position, gotoIdle, wanderComp.parent.Map)) return null;
            if (!wanderComp.IPawn.CanWander) return null;
            JobDef job = DefDatabase<JobDef>.GetNamed("WanderAtParent");
            return JobMaker.MakeJob(job, gotoIdle);
        }
    }

    public class JobDriver_WanderAtParent : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoIdle = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            gotoIdle.FailOn(() => !pawn.CanReach(TargetA, PathEndMode.OnCell, Danger.Deadly));
            gotoIdle.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            yield return gotoIdle;
        }
    }
}
