using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class Hediff_RegenerativeNanites : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (totalDamageDealt <= 0) return;

        }

        private void ApplyPotentialHealing()
        {
            foreach (var bodyPart in pawn.health.hediffSet.GetInjuredParts())
            {
                pawn.health.AddHediff(TRHediffDefOf.RegenerativeNanites, bodyPart);
            }
        }
    }
}
