using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class JobGiver_ReturnToRefineryToIdle : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Harvester harvester = pawn as Harvester;
            IntVec3 gotoIdle = harvester.Position;
            if (harvester.ShouldIdle)
            {
                if (harvester.CanReach(harvester.AvailableRefinery, PathEndMode.Touch, Danger.Deadly))
                {                   
                    gotoIdle = harvester.AvailableRefinery.InteractionCell;
                }
                JobDef job = DefDatabase<JobDef>.GetNamed("ReturnToRefineryToIdle");
                return new Job(job, gotoIdle);
            }
            return null;
        }
    }

    public class JobDriver_ReturnToRefineryToIdle : JobDriver
    {
        private Harvester Harvester
        {
            get
            {
                return this.pawn as Harvester;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            Toil idle = new Toil();
            idle.initAction = delegate
            {
                Harvester actor = idle.actor as Harvester;
                actor.pather.StopDead();
                actor.Rotation = actor.AvailableRefinery != null ? actor.AvailableRefinery.Rotation.Opposite : Rot4.Random;
            };
            idle.tickAction = delegate
            {
                Harvester actor = idle.actor as Harvester;
                if (actor.ShouldHarvest)
                {
                    this.EndJobWith(JobCondition.InterruptForced);
                    return;
                }
                if(actor.AvailableRefinery != null)
                {
                    if(actor.Position != actor.AvailableRefinery.InteractionCell)
                    {
                        this.EndJobWith(JobCondition.InterruptForced);
                        return;
                    }
                }
            };
            idle.FailOnDespawnedOrNull(TargetIndex.A);
            idle.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            idle.defaultCompleteMode = ToilCompleteMode.Never;
            yield return idle;
        }
    }
}
