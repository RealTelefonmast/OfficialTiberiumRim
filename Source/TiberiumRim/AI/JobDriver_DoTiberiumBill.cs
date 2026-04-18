using System.Collections.Generic;
using RimWorld;
using TR.Networks.TiberiumNetwork;
using Verse.AI;

namespace TR;

public class JobDriver_DoTiberiumBill : JobDriver
{
    public CompTNS_Crafter Crafter => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompTNS_Crafter>();

    public CustomTiberiumBill CurrentBill => Crafter.BillStack.CurrentBill;

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
        billToil.initAction = delegate { };
        billToil.tickAction = delegate
        {
            var bill = CurrentBill;
            var pawn = billToil.actor;
            bill.DoWork(pawn);
            if (bill.TryFinish()) bill.Pay();
        };
        billToil.defaultCompleteMode = ToilCompleteMode.Never;
        billToil.WithEffect(() => EffecterDefOf.ConstructMetal, TargetIndex.A);
        //billToil.PlaySustainerOrSound(() => SoundDefOf.);
        billToil.WithProgressBar(TargetIndex.A, () => 1 - CurrentBill.WorkLeft / CurrentBill.workAmountTotal);
        yield return billToil;
    }
}