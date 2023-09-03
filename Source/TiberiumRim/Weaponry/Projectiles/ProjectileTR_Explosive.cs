﻿using TeleCore;
using UnityEngine;
using Verse;

namespace TR
{
    public class ProjectileTR_Explosive : Projectile_Explosive, IPatchedProjectile
    {
        public TRThingDef TRDef => base.def as TRThingDef;
        public ProjectileDefExtension Props => TRDef?.TeleExtension().projectile;

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (Props != null)
            {
                Props.impactExplosion?.DoExplosion(Position, Map, this);
                Props.impactFilth?.SpawnFilth(Position, Map);
                Props.impactEffecter?.Spawn(Position, Map);
            }
            base.Impact(hitThing);
        }

        #region  PATCH BEHAVIOUR

        public float ArcHeightFactorPostAdd => 0;

        public bool PreLaunch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            return true;
        }

        public void PostLaunch(ref Vector3 origin, ref Vector3 destination)
        {
            
        }

        public void CanHitOverride(Thing thing, ref bool result)
        {
        }

        public bool PreImpact()
        {
            return true;
        }

        public void PostImpact()
        {
        }

        #endregion
    }
}
