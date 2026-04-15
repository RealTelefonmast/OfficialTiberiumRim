using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class JobGiver_WanderAtParent : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            RepairDrone drone = pawn as RepairDrone;
            IntVec3 gotoIdle = drone.parent.Position + GenRadial.RadialPattern[Rand.Range(0, drone.RadialCells)];
            if (GenSight.LineOfSight(drone.parent.Position, gotoIdle, drone.Map))
            {
                if (drone.AvailableMech == null)
                {
                    JobDef job = DefDatabase<JobDef>.GetNamed("WanderAtParent");
                    return new Job(job, gotoIdle);
                }
            }
            return null;
        }
    }

    public class JobDriver_WanderAtParent : JobDriver
    {
        public override bool TryMakePreToilReservations()
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
