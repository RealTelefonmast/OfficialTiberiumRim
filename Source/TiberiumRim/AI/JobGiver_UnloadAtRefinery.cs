using System.Collections.Generic;
using RimWorld;
using TeleCore.Network.Flow.Values;
using TR.Defs;
using TR.GameParts.Networks.TiberiumNetwork;
using TR.TiberiumObjects;
using TR.TiberiumProcessing;
using TR.Util;
using Verse;
using Verse.AI;

namespace TR.AI;

public class JobGiver_UnloadAtRefinery : ThinkNode_JobGiver
{
    public override Job TryGiveJob(Pawn pawn)
    {
        var harvester = pawn as Harvester;
        if (harvester.CurrentPriority != HarvesterPriority.Unload) return null;
        if (harvester.IsUnloading) return null;
        //
        if (harvester.RefineryComp.HarvesterCount > 1)
            if (!harvester.AtRefinery && harvester.Refinery.IsReserved(out var claimant) && claimant != harvester)
                return JobMaker.MakeJob(JobDefOf.Goto, harvester.Refinery.InteractionCell);

        if (harvester.CanReserveAndReach(harvester.Refinery, PathEndMode.InteractionCell, Danger.Deadly))
        {
            var job = DefDatabase<JobDef>.GetNamed("UnloadAtRefinery");
            return JobMaker.MakeJob(job, harvester.Refinery);
        }

        return null;
    }
}

public class JobDriver_UnloadAtRefinery : JobDriver
{
    private CompTNS_Refinery RefineryComp => ((Building)TargetA.Thing).GetComp<CompTNS_Refinery>();
    private Harvester Harvester => (Harvester)pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.CanReserve(TargetA) && pawn.Reserve(TargetA, job);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        var gotoToil = Toils_Goto.GotoCell(TargetA.Thing.InteractionCell, PathEndMode.OnCell);
        gotoToil.FailOnDespawnedOrNull(TargetIndex.A);
        yield return gotoToil;

        var unload = new Toil();
        unload.initAction = delegate
        {
            Harvester.pather.StopDead();
            Harvester.Rotation = RefineryComp.parent.Rotation.Opposite;
        };
        unload.tickAction = delegate
        {
            var refineryVolume = RefineryComp?.Container;
            if (refineryVolume == null) { EndJobWith(JobCondition.Errored); return; }

            if (refineryVolume.FillPercent < 1f)
            {
                if (Harvester.Container.TotalStorage > 0f)
                {
                    var mainType = Harvester.Container.MainValueType;
                    var networkDef = TiberiumValueTypeToNetworkDef(mainType);
                    if (networkDef != null &&
                        Harvester.Container.TryRemoveValue(mainType, Harvester.kindDef.unloadValue, out var actual))
                        refineryVolume.TryAdd(networkDef, actual);
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
        unload.FailOn(() => !((Building)TargetA.Thing).IsPowered(out var usesPower) && usesPower);
        unload.defaultCompleteMode = ToilCompleteMode.Never;
        yield return unload;
    }

    private static NetworkValueDef TiberiumValueTypeToNetworkDef(TiberiumValueType type) => type switch
    {
        TiberiumValueType.Green => TiberiumDefOf.TibGreen,
        TiberiumValueType.Blue  => TiberiumDefOf.TibBlue,
        TiberiumValueType.Red   => TiberiumDefOf.TibRed,
        TiberiumValueType.Sludge => TiberiumDefOf.TibSludge,
        TiberiumValueType.Gas   => TiberiumDefOf.TibGas,
        _ => null
    };
}
