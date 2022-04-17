using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumChunk : TRThing
    {
        public override void TickRare()
        {
            base.TickRare();
            if (TRandom.Chance(0.1f))
                this.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, TRandom.Range(0, 3)));
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }
    }
}
