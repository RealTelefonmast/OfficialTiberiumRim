using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobDriver_RepairDroneRepair : JobDriver
    {
        private Comp_DroneStation DroneStation => Drone.parentComp;
        private RepairDrone Drone => this.pawn as RepairDrone;
        private MechanicalPawn Target => this.TargetA.Thing as MechanicalPawn;
        private List<Hediff> Hediffs => (this.job as JobWithExtras).hediffs;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(this.TargetA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);
            this.FailOn(() => !DroneStation.FuelComp.HasFuel || (Target.Position.DistanceTo(DroneStation.parent.Position) > DroneStation.Props.radius));
            Toil gotoTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return gotoTarget;
            Toil repair = new Toil();
            repair.initAction = delegate
            {
                Drone.pather.StopDead();
            };
            repair.tickAction = delegate
            {
                var injury = Hediffs.First();
                if (injury?.Severity > 0)
                {
                    if (Target.health.hediffSet.PartIsMissing(injury.Part))
                    {
                        Target.health.RestorePart(injury.Part);
                        return;
                    }
                    injury.Heal(Drone.kindDef.healFloat);
                    DroneStation.FuelComp.ConsumeFuel(Drone.kindDef.healFloat);
                }
                else
                {
                    Hediffs.Remove(injury);
                }
                if (!Hediffs.NullOrEmpty()) return;
                Target.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                repair.actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
            };
            repair.WithEffect(TargetThingA.def.repairEffect, TargetIndex.A);
            repair.defaultCompleteMode = ToilCompleteMode.Never;
            repair.JumpIf(() => !this.GetActor().CanReachImmediate(this.GetActor().jobs.curJob.GetTarget(TargetIndex.A), PathEndMode.Touch), gotoTarget);
            yield return repair;
        }
    }
}
