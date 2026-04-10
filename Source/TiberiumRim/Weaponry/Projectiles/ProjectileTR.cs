using TeleCore.Interfaces;
using UnityEngine;
using Verse;

namespace TR.Projectiles;

public class ProjectileTR : Projectile, IPatchedProjectile
{
    public TRThingDef TRDef => def as TRThingDef;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
    }

    public override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        base.Impact(hitThing, blockedByShield);
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
    }

    #region PATCH BEHAVIOUR

    public float ArcHeightFactorPostAdd => 0;

    public virtual bool PreLaunch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget,
        LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false,
        Thing equipment = null, ThingDef targetCoverDef = null)
    {
        return true;
    }

    public virtual void PostLaunch(ref Vector3 origin, ref Vector3 destination)
    {
    }

    public virtual void CanHitOverride(Thing thing, ref bool result)
    {
    }

    public virtual bool PreImpact()
    {
        return true;
    }

    public virtual void PostImpact()
    {
    }

    #endregion
}