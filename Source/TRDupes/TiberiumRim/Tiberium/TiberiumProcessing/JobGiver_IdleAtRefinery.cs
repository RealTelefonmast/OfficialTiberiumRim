using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class JobGiver_IdleAtRefinery : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Harvester harvester = pawn as Harvester;
            if (harvester.ShouldIdle)
            {
                return new Job(TiberiumDefOf.IdleAtRefinery, harvester.IdlePos);
            }
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
            Toil idle = new Toil();
            idle.initAction = delegate
            {
                Harvester actor = idle.actor as Harvester;
                actor.pather.StopDead();
                actor.Rotation = !actor.MainRefinery.DestroyedOrNull() ? actor.MainRefinery.Rotation.Opposite : Rot4.Random;
            };
            idle.tickAction = delegate
            {
                Harvester actor = idle.actor as Harvester;
                if (actor.ShouldHarvest)
                {
                    this.EndJobWith(JobCondition.InterruptForced);
                    return;
                }
                if (!actor.MainRefinery.DestroyedOrNull())
                {
                    if (actor.Position != actor.MainRefinery.InteractionCell)
                    {
                        this.EndJobWith(JobCondition.InterruptForced);
                        return;
                    }
                }
            };
            idle.FailOn(() => !((Harvester)pawn).ShouldIdle);
            idle.FailOnDespawnedOrNull(TargetIndex.A);
            idle.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            idle.defaultCompleteMode = ToilCompleteMode.Never;
            yield return idle;
        }
    }
}
