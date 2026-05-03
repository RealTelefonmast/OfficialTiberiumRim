using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim;

public class JobGiver_UnloadAtRefinery : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        TR.Harvester harvester = pawn as TR.Harvester;
        if (harvester.ShouldUnload)
        {
            TNW_Refinery refinery = harvester.RefineryToUnload;
            if (!refinery.DestroyedOrNull() && refinery.GetComp<CompPowerTrader>().PowerOn)
                if (harvester.CanReserveAndReach(refinery, PathEndMode.InteractionCell, Danger.Deadly))
                {
                    var job = DefDatabase<JobDef>.GetNamed("UnloadAtRefinery");
                    return new Job(job, refinery);
                }
        }

        return null;
    }
}

public class JobDriver_UnloadAtRefinery : JobDriver
{
    private TNW_Refinery Refinery => (TNW_Refinery)TargetA.Thing;

    private TR.Harvester Harvester => (TR.Harvester)pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (pawn.CanReserve(TargetA)) return pawn.Reserve(TargetA, job);
        return false;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var gotoToil = Toils_Goto.GotoCell(TargetA.Thing.InteractionCell, PathEndMode.OnCell);
        gotoToil.FailOnDespawnedOrNull(TargetIndex.A);
        yield return gotoToil;
        var unload = new Toil();
        unload.initAction = delegate
        {
            Harvester.pather.StopDead();
            Harvester.Rotation = Refinery.Rotation.Opposite;
        };
        unload.tickAction = delegate
        {
            if (!Refinery.Container.CapacityFull)
            {
                if (Harvester.Container.StoredPct > 0f)
                    Harvester.Container.TryTransferTo(Refinery.Container, Harvester.Container.MainType,
                        (int)Refinery.def.flowAmount);
                else
                    EndJobWith(JobCondition.Succeeded);
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