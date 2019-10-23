using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class JobGiver_UnloadAtRefinery : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Harvester harvester = pawn as Harvester;
            if (harvester.ShouldUnload)
            {
                CompTNW_Refinery refinery = harvester.CurrentRefinery;
                if (refinery != null)
                {
                    if(harvester.CanReserveAndReach(refinery.parent, PathEndMode.InteractionCell, Danger.Deadly))
                    {
                        JobDef job = DefDatabase<JobDef>.GetNamed("UnloadAtRefinery");
                        return new Job(job, refinery.parent);
                    }
                }
            }
            return null;
        }
    }

    public class JobDriver_UnloadAtRefinery : JobDriver
    {
        private CompTNW Refinery
        {
            get
            {
                return Harvester.CurrentRefinery;
            }
        }

        private Harvester Harvester
        {
            get
            {
                return (Harvester)pawn;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.CanReserve(TargetA))
            {
                return pawn.Reserve(TargetA, job);
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoToil = Toils_Goto.GotoCell(TargetA.Thing.InteractionCell, PathEndMode.OnCell);
            gotoToil.FailOnDespawnedOrNull(TargetIndex.A);
            yield return gotoToil;
            Toil unload = new Toil();
            unload.initAction = delegate
            {
                Harvester.pather.StopDead();
                Harvester.Rotation = Refinery.parent.Rotation.Opposite;
            };
            unload.tickAction = delegate
            {
                if (!Refinery?.Container.CapacityFull ?? false)
                {
                    if (Harvester.Container.StoredPercent > 0f)
                    {
                        Harvester.Container.TryTransferTo(Refinery.Container, Harvester.Container.MainValueType, Harvester.kindDef.unloadValue);
                    }
                    else
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
                else
                {
                    EndJobWith(JobCondition.InterruptForced);
                }
            };
            unload.FailOnDespawnedOrNull(TargetIndex.A);
            unload.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            unload.defaultCompleteMode = ToilCompleteMode.Never;
            yield return unload;
        }
    }
}
