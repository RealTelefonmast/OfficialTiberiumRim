using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobDriver_DoTiberiumBill : JobDriver
    {
        public CompTNW_Crafter Crafter => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompTNW_Crafter>();

        public CustomTiberiumBill CurrentBill => Crafter.BillStack.CurrentBill;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job))
            {
                return false;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.InteractionCell);
            var billToil = new Toil();
            billToil.FailOn(() => CurrentBill == null || !CurrentBill.ShouldDoNow());
            billToil.initAction = delegate
            {

            };
            billToil.tickAction = delegate
            {
                var bill = CurrentBill;
                Pawn pawn = billToil.actor;
                bill.DoWork(pawn);
                if (bill.TryFinish())
                {
                    bill.Pay();
                }
            };
            billToil.defaultCompleteMode = ToilCompleteMode.Never;
            billToil.WithEffect(() => EffecterDefOf.ConstructMetal, TargetIndex.A);
            //billToil.PlaySustainerOrSound(() => SoundDefOf.);
            billToil.WithProgressBar(TargetIndex.A, () => 1 - (CurrentBill.WorkLeft / CurrentBill.workAmountTotal), false, -0.5f);
            yield return billToil;
        }
    }
}
