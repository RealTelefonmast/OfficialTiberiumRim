﻿using Verse;

namespace TR
{
    public class Hediff_CauseToxemia : HediffWithComps
    {
        protected Hediff_TiberiumToxemia parent;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (!pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberiumToxemia))
                parent = (Hediff_TiberiumToxemia)pawn.health.AddHediff(TRHediffDefOf.TiberiumToxemia);

        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            //if (pawn.health.hediffSet.GetHediffs<Hediff_CauseToxemia>().Any(h => h != this)) return;
            //pawn.health.RemoveHediff(parent);
        }
    }
}
