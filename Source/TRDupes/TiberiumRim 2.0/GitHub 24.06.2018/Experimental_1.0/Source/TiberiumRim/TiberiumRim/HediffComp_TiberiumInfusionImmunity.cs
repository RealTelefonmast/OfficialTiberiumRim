using Verse;

namespace TiberiumRim
{
    public class HediffComp_TiberiumInfusionImmunity : HediffComp
    {
        public HediffCompProperties_TiberiumInfusionImmunity Props
        {
            get
            {
                return (HediffCompProperties_TiberiumInfusionImmunity)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (base.Pawn.IsHashIntervalTick(GenTicks.TickRareInterval))
            {
                Hediff hediff = TRUtils.TibSickness(this.Pawn);
                if (hediff != null)
                {
                    hediff.Severity -= 0.1f;
                }
                if (TRUtils.GetMutation(this.Pawn) != null)
                {
                    this.Pawn.health.AddHediff(HediffDef.Named("TiberiumMutationCure"));
                }
            }
        }
    }

    public class HediffCompProperties_TiberiumInfusionImmunity : HediffCompProperties
    {
        public HediffCompProperties_TiberiumInfusionImmunity()
        {
            this.compClass = typeof(HediffComp_TiberiumInfusionImmunity);
        }
    }
}
