using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class ThingComp_TiberiumCheck : ThingComp
    {
        private int ticker = 0;
        private Pawn Pawn => parent as Pawn;

        public override void CompTick()
        {
            base.CompTick();
            if (!Pawn.Spawned)
                return;
            if (ticker <= 0)
            {
                //Log.Message("Ticking TiberiumCheck on " + Pawn.LabelCap);
                var tib = Pawn.Position.GetTiberium(Pawn.Map);
                if (tib?.def.IsInfective ?? false)
                    HediffUtils.TryAffectPawn(Pawn, false);
                ticker = 250;
            }
            ticker--;
        }
    }

    public class CompProperties_TiberiumCheck : CompProperties
    {
        public CompProperties_TiberiumCheck()
        {
            this.compClass = typeof(ThingComp_TiberiumCheck);
        }
    }
}
