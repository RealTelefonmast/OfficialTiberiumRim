using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class JobDriver_RepairDroneRepair : JobDriver
    {
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
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
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
            yield return repair;
        }
    }
}
