using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class HediffComp_Mutation : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (base.Pawn.IsHashIntervalTick(GenTicks.TickRareInterval))
            {
                CheckSeverity(this.Pawn);
            }
        }

        public HediffCompProperties_TiberiumMutation Props
        {
            get
            {
                return (HediffCompProperties_TiberiumMutation)this.props;
            }
        }

        public void CheckSeverity(Pawn p)
        {
            HediffDef Infection = TiberiumHediffDefOf.TiberiumInfection;
            HediffDef MutationGood = TiberiumHediffDefOf.TiberiumMutationGood;
            HediffDef MutationBad = TiberiumHediffDefOf.TiberiumMutationBad;
            HediffDef Addiction = TiberiumHediffDefOf.TiberiumAddiction;
            HediffDef Exposure = TiberiumHediffDefOf.TiberiumExposure;

            if (!p.health.hediffSet.HasHediff(Infection) && this.parent.Severity > 0.3 && Rand.Chance(0.1f))
            {

                List<BodyPartRecord> list = new List<BodyPartRecord>();

                foreach (BodyPartRecord i in p.RaceProps.body.AllParts)
                {
                    if (i.depth == BodyPartDepth.Outside && !p.health.hediffSet.PartIsMissing(i))
                    {
                        list.Add(i);
                    }
                }

                BodyPartRecord target = null;
                target = list.RandomElement();

                p.health.AddHediff(Infection, target, null);
                p.health.RemoveHediff(this.parent);
                HealthUtility.AdjustSeverity(p, Exposure, -1.5f);
                return;
            }
            else if (!p.health.hediffSet.HasHediff(MutationGood) && this.parent.Severity > 0.8 && Rand.Chance(0.7f))
            {
                p.health.AddHediff(MutationGood);
                p.health.AddHediff(Addiction);
                p.health.RemoveHediff(this.parent);
                HealthUtility.AdjustSeverity(p, Exposure, -1.5f);
                return;
            }
            else if (!p.health.hediffSet.HasHediff(MutationBad) && this.parent.Severity > 0.5 && Rand.Chance(0.1f))
            {
                p.health.AddHediff(MutationBad);
                p.health.AddHediff(Addiction);
                p.health.RemoveHediff(this.parent);
                HealthUtility.AdjustSeverity(p, Exposure, -1.5f);
            }
            else if (p.kindDef.defName.Contains("Tribesperson"))
            {
                if (Rand.Chance(0.25f))
                {
                    p.health.AddHediff(MutationGood);
                    p.health.AddHediff(Addiction);
                    p.health.RemoveHediff(this.parent);
                    HealthUtility.AdjustSeverity(p, Exposure, -1.5f);
                    return;
                }
                else
                {
                    p.health.AddHediff(MutationBad);
                    p.health.AddHediff(Addiction);
                    p.health.RemoveHediff(this.parent);
                    HealthUtility.AdjustSeverity(p, Exposure, -1.5f);
                }
            }

        }
    }

    public class HediffCompProperties_TiberiumMutation : HediffCompProperties
    {
        public HediffCompProperties_TiberiumMutation()
        {
            this.compClass = typeof(HediffComp_Mutation);
        }
    }
}
