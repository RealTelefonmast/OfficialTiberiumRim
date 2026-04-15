using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class HediffComp_TiberiumReverseMutation : HediffComp
    {
        public HediffCompProperties_TiberiumReverseMutation Props
        {
            get
            {
                return (HediffCompProperties_TiberiumReverseMutation)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (base.Pawn.IsHashIntervalTick(GenTicks.TickRareInterval))
            {
                Hediff hediff = TRUtils.GetMutation(this.Pawn);
                if (hediff != null)
                {
                    hediff.Severity -= 0.01f;
                }
                else
                {
                    Need N = this.Pawn.needs.AllNeeds.Find((Need x) => x.def.defName.Contains("Need_Tiberium"));
                    Hediff need = this.Pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == TiberiumHediffDefOf.TiberiumAddiction);
                    this.Pawn.needs.AllNeeds.Remove(N);
                    this.Pawn.health.hediffSet.hediffs.Remove(need);
                    this.parent.Severity = 0;
                }
            }
        }
    }

    public class HediffCompProperties_TiberiumReverseMutation : HediffCompProperties
    {
        public HediffCompProperties_TiberiumReverseMutation()
        {
            this.compClass = typeof(HediffComp_TiberiumReverseMutation);
        }
    }
}
