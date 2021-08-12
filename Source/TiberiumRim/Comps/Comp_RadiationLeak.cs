using Verse;

namespace TiberiumRim
{
    public class Comp_RadiationLeak : ThingComp_TiberiumRadiation, IRadiationSource
    {
        public IRadiationLeaker Leaker => (IRadiationLeaker)parent;

        private bool leakCaused = false;

        private bool ShouldLeakWhenDamaged => Props.leakDamageThreshold > 0;

        protected override bool ShouldRadiate => Leaker?.CauseLeak ?? false;
        protected override bool ShouldGlow => Leaker?.CauseLeak ?? false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref leakCaused, "leakCaused");
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            if (ShouldLeakWhenDamaged && !parent.DestroyedOrNull() && (float)parent.HitPoints / parent.MaxHitPoints <= Props.leakDamageThreshold)
            {
                TryStartRadiating();
            }
        }
    }
}
