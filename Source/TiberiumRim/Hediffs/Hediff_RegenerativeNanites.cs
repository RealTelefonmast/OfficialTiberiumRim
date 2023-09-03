using Verse;

namespace TR
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
