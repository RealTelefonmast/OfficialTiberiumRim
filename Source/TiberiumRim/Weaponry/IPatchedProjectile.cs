using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public interface IPatchedProjectile
    {
        public ProjectileProperties_Extended Props { get; }
        public float ArcHeightFactorPostAdd { get; }
        public bool PreLaunch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null);
        public void PostLaunch(ref Vector3 origin, ref Vector3 destination);
        public bool CanHitOverride(Thing thing, ref bool result);
        bool PreImpact();
        void PostImpact();
    }
}
