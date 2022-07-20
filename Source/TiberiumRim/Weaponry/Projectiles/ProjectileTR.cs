using System;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ProjectileTR : Projectile, IPatchedProjectile
    {
        public TRThingDef TRDef => base.def as TRThingDef;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
        }

        public override void Draw()
        {
            base.Draw();
        }

        #region  PATCH BEHAVIOUR

        public float ArcHeightFactorPostAdd => 0;

        public virtual bool PreLaunch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
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
}
