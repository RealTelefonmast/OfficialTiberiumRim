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
    public class JobGiver_RepairAnyMechWithinRange : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            RepairDrone drone = pawn as RepairDrone;
            Mechanical_Pawn mech = drone.AvailableMech;
            if (mech != null)
            {
                if (pawn.CurJob != null && pawn.CurJob.targetA != mech)
                {
                    if (pawn.CanReserveAndReach(mech, PathEndMode.Touch, Danger.Deadly))
                    {
                        JobDef job = DefDatabase<JobDef>.GetNamed("RepairMechanicalPawn");
                        return new Job(job, mech);
                    }
                }
            }
            return null;
        }
    }

    public class JobDriver_RapairAnyMechWithinRange : JobDriver
    {
        private RepairDrone Drone
        {
            get
            {
                return this.pawn as RepairDrone;
            }
        }

        private Mechanical_Pawn Target
        {
            get
            {
                return this.TargetA.Thing as Mechanical_Pawn;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.TargetA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);
            this.FailOnNotCasualInterruptible(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil repair = new Toil();
            repair.initAction = delegate
            {
                Drone.pather.StopDead();
            };
            repair.tickAction = delegate
            {
                if ((from x in Target?.health?.hediffSet?.GetHediffs<Hediff>()
                     select x).TryRandomElement(out Hediff hediff_Injury))
                {
                    if (hediff_Injury != null)
                    {
                        if (Target.health.hediffSet.PartIsMissing(hediff_Injury.Part))
                        {
                            Target.health.RestorePart(hediff_Injury.Part);
                            return;
                        }
                        hediff_Injury.Heal(Drone.kindDef.healFloat);
                    }
                }
                if (Target?.health?.summaryHealth?.SummaryHealthPercent >= 1f)
                {
                    Target.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                    repair.actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                }
            };
            repair.WithEffect(TargetThingA.def.repairEffect, TargetIndex.A);
            repair.defaultCompleteMode = ToilCompleteMode.Never;
            yield return repair;
        }
    }
}
