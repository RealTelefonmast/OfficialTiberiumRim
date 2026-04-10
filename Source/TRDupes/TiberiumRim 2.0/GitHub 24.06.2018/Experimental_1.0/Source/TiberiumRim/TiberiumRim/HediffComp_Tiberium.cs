using Verse;

namespace TiberiumRim
{
    public class HediffComp_Tiberium : HediffComp
    {
        private HediffDef Stage1 = TiberiumHediffDefOf.TiberiumStage1;
        private HediffDef Stage2 = TiberiumHediffDefOf.TiberiumStage2;
        private TiberiumControlDef TCD = MainTCD.MainTiberiumControlDef;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (base.Pawn.IsHashIntervalTick(GenTicks.TickRareInterval))
            {
                CheckSeverity(this.Pawn);
            }
        }

        public void CheckSeverity(Pawn p)
        {         
            if (!p.health.hediffSet.HasHediff(Stage1) && !p.health.hediffSet.HasHediff(Stage2) && this.parent.Severity > 0.3 && Rand.Chance(0.3f))
            {
                if (p.AnimalOrWildMan())
                {
                    HealthUtility.AdjustSeverity(p, this.parent.def, -0.5f);
                    return;
                }
                HealthUtility.AdjustSeverity(p, this.parent.def, -0.5f);
                HealthUtility.AdjustSeverity(p, Stage1, 1.0f);
                return;
            }

            if (!p.health.hediffSet.HasHediff(Stage2) && this.parent.Severity > 0.7 && Rand.Chance(0.5f))
            {
                HealthUtility.AdjustSeverity(p, Stage2, 0.2f);
                p.health.RemoveHediff(this.parent);

                Hediff R = p.health.hediffSet.hediffs.Find((Hediff x) => x.def == TiberiumHediffDefOf.TiberiumStage1);
                if (R != null)
                {
                    p.health.RemoveHediff(R);
                }
                return;
            }
        }
    }

    public class HediffCompProperties_Tiberium : HediffCompProperties
    {
        public HediffCompProperties_Tiberium()
        {
            this.compClass = typeof(HediffComp_Tiberium);
        }
    }
}
