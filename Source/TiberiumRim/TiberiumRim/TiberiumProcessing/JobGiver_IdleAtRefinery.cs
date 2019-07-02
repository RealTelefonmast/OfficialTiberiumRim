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

        private Harvester Harvester
        {
            get
            {
                return pawn as Harvester;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            Toil idle = new Toil();
            idle.initAction = delegate
            {
                Harvester actor = idle.actor as Harvester;
                actor.pather.StopDead();
                actor.Rotation = actor.Refinery?.Rotation.Opposite ?? Rot4.Random;
            };
            idle.tickAction = delegate
            {
            };
            idle.FailOn(() => !Harvester.ShouldIdle || Harvester.MainRefineryLost);
            idle.FailOnDespawnedOrNull(TargetIndex.A);
            idle.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            idle.defaultCompleteMode = ToilCompleteMode.Never;
            yield return idle;
        }
    }
}
