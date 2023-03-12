using System.Collections.Generic;
using RimWorld;
using TeleCore;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobGiver_UnloadAtRefinery : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            Harvester harvester = pawn as Harvester;
            if (harvester.CurrentPriority != HarvesterPriority.Unload) return null;
            if (harvester.IsUnloading) return null;
            //
            if (harvester.RefineryComp.HarvesterCount > 1)
            {
                //if(!)
                if (!harvester.AtRefinery && harvester.Refinery.IsReserved(pawn.Map, out Pawn claimant) && claimant != harvester)
                {
                    return JobMaker.MakeJob(JobDefOf.Goto, harvester.Refinery.InteractionCell);
                }
            }
            if(harvester.CanReserveAndReach(harvester.Refinery, PathEndMode.InteractionCell, Danger.Deadly))
            {
                JobDef job = DefDatabase<JobDef>.GetNamed("UnloadAtRefinery");
                return JobMaker.MakeJob(job, harvester.Refinery);
            }
            return null;
        }
    }

    public class JobDriver_UnloadAtRefinery : JobDriver
    {
        private Comp_TiberiumNetworkStructure Refinery => Harvester.RefineryComp;

        private NetworkSubPart RefineryComp => Refinery[TiberiumDefOf.TiberiumNetwork];

        private Harvester Harvester => (Harvester)pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.CanReserve(TargetA) && pawn.Reserve(TargetA, job);
        }

        public override IEnumerable<Toil> MakeNewToils()
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
                if (!RefineryComp?.Container.Full ?? false)
                {
                    if (Harvester.Container.StoredPercent > 0f)
                    {
                        //RefineryComp.Container, Harvester.Container.CurrentMainValueType, Harvester.kindDef.unloadValue, out _
                        Harvester.Container.TryTransferValue(RefineryComp.Container, Harvester.Container.CurrentMainValueType, Harvester.kindDef.unloadValue, out _);
                        //Harvester.Container.TryTransferTo(RefineryComp.Container, Harvester.kindDef.unloadValue, out _);
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
            unload.FailOn(() => !((Building) TargetA.Thing).IsElectricallyPowered(out _));
            unload.defaultCompleteMode = ToilCompleteMode.Never;
            yield return unload;
        }
    }
}
