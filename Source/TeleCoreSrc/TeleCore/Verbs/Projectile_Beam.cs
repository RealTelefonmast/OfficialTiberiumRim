using UnityEngine;
using Verse;

namespace TeleCore.Verbs;

//A lot of beam weaponry just uses a hitscan projectile with custom drawing.
//Sustained beams would require a custom Verb.
public class Projectile_Beam : Projectile
{
    public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
    {
        base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
    }

    public override void Tick()
    {
        base.Tick();
    }

    //Draw beam and fade-out
    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
    }
}