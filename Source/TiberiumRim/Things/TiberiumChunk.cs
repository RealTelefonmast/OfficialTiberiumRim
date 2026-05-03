using RimWorld;
using Verse;

namespace TiberiumRim;

public class TiberiumChunk : TiberiumThing
{
    public override void TickRare()
    {
        base.TickRare();
        if (TRUtils.Chance(0.1f))
            TakeDamage(new DamageInfo(DamageDefOf.Deterioration, TRUtils.Range(0, 3)));
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        base.Destroy(mode);
    }
}