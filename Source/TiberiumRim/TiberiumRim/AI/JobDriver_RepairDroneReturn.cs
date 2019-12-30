using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class ThinkNode_ConditionalRepair : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            var drone = pawn as RepairDrone;
            var comp = drone.parentComp;
            if (!comp.MechsAvailableForRepair().Any()) return false;
            return pawn.CurJobDef != DefDatabase<JobDef>.GetNamed("RepairMechanicalPawn");
        }
    }

    public class JobGiver_RepairDroneReturn : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            var jobDef = DefDatabase<JobDef>.GetNamed("ReturnFromRepair");
            var drone = pawn as RepairDrone;
            var job = new Job(jobDef, drone.ParentBuilding);
            return job;
        }
    }

    public class JobDriver_RepairDroneReturn : JobDriver
    {
        private RepairDrone Drone => this.pawn as RepairDrone;
        private Comp_RepairDrone RepairComp => Drone.parentComp;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            Toil repair = new Toil();
            repair.initAction = delegate
            {
                RepairComp.StoreDrone(Drone);
            };
            yield return repair;
        }
    }
}
