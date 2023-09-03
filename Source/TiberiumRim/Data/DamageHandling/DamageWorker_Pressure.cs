using System.Collections.Generic;
using Verse;

namespace TR;

public class DamageWorker_Pressure : DamageWorker_AddInjury
{
    public override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
    {
        return base.ChooseHitPart(dinfo, pawn);
    }

    public override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
    {
        base.ExplosionDamageThing(explosion, t, damagedThings, ignoredThings, cell);
    }
}