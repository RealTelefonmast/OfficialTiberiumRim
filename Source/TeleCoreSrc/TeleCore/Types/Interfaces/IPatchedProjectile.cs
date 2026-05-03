using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public interface IPatchedProjectile
{
    public float ArcHeightFactorPostAdd { get; }

    public bool PreLaunch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget,
        ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null,
        ThingDef targetCoverDef = null);

    public void PostLaunch(ref Vector3 origin, ref Vector3 destination);
    public void CanHitOverride(Thing thing, ref bool result);
    bool PreImpact();
    void PostImpact();
}