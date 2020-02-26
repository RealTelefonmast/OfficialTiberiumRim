using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public class JobGiver_RepairDrone_WanderAtParent : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Comp_RepairDrone drone = (pawn as RepairDrone).parentComp;
            IntVec3 gotoIdle = drone.parent.Position + GenRadial.RadialPattern[Rand.Range(0, drone.radialCells)];
            if (!GenSight.LineOfSight(drone.parent.Position, gotoIdle, pawn.Map)) return null;
            if (!(pawn as IPawnWithParent).CanWander) return null;
            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("WanderAtParent"), gotoIdle);
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
