using System.Collections.Generic;
using RimWorld;
using TeleCore.FlowCore;
using TeleCore.FlowCore.Bills;
using Verse;
using Verse.AI;

namespace TeleCore.RimWorld.AI;

public class JobDriver_DoNetworkBill : JobDriver
{
    public Comp_NetworkBillsCrafter Crafter =>
        job.GetTarget(TargetIndex.A).Thing.TryGetComp<Comp_NetworkBillsCrafter>();

    public CustomNetworkBill CurrentBill => Crafter.BillStack.CurrentBill;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job)) return false;
        return true;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.InteractionCell);
        var billToil = new Toil();
        billToil.FailOn(() => CurrentBill == null || !CurrentBill.ShouldDoNow());
        billToil.tickAction = delegate
        {
            var actor = billToil.actor;
            var curJob = actor.jobs.curJob;
            var bill = CurrentBill;
            var pawn = billToil.actor;
            bill.DoWork(pawn);
            if (bill.TryFinish(out var results))
            {
                //
                var invalid = IntVec3.Invalid;
                if (bill.StoreMode == BillStoreModeDefOf.BestStockpile)
                    StoreUtility.TryFindBestBetterStoreCellFor(results[0], actor, actor.Map, StoragePriority.Unstored,
                        actor.Faction, out invalid);
                else if (bill.StoreMode == BillStoreModeDefOf.SpecificStockpile)
                    StoreUtility.TryFindBestBetterStoreCellForIn(results[0], actor, actor.Map, StoragePriority.Unstored,
                        actor.Faction, bill.StoreZone.slotGroup, out invalid);
                if (invalid.IsValid)
                {
                    actor.carryTracker.TryStartCarry(results[0]);
                    curJob.targetB = invalid;
                    curJob.targetA = results[0];
                    curJob.count = 99999;
                }

                EndJobWith(JobCondition.Succeeded);
            }
        };
        billToil.defaultCompleteMode = ToilCompleteMode.Never;
        billToil.WithEffect(() => EffecterDefOf.ConstructMetal, TargetIndex.A);
        //billToil.PlaySustainerOrSound(() => SoundDefOf.);
        billToil.WithProgressBar(TargetIndex.A, () => 1 - CurrentBill.WorkLeft / CurrentBill.workAmountTotal);
        yield return billToil;

        //
        yield return Toils_Reserve.Reserve(TargetIndex.B);
        var carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
        yield return carryToCell;
        yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true, true);
    }
}