using UnityEngine;
using Verse;

namespace TiberiumRim
{
    //TODO: Implement IPatchedProjectile
    public class Projectile_Homing : ProjectileTR
    {
        private float speed = 1f;
        private Vector3? exactPos;

        private LocalTargetInfo offsetTarget, actualTarget;


        [TweakValue("HOMING_OSC", 0f, 2f)]
        public static float WiggleValue = 0.5f;

        [TweakValue("HOMING_OSC_TIME", 1, 100)]
        public static int OSC_TICKS = 25;

        private Vector3? initVector;
        private int startTick = 0;
        private int tickOffset = 0;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            speed = this.def.projectile.speed;
            tickOffset = Rand.Range(0, 100);
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref exactPos, "exactPos");
            base.ExposeData();
        }

        public override void Tick()
        {

            startTick++;

            //Get initial directional speed vector
            initVector ??= PushVelocity;
            exactPos ??= origin;

            exactPos += ((PullVelocity + PushVelocity) * (0.0166666675f * 1));
            if(ActualPosition.DistanceTo(actualTarget.Cell) <= 0.5f)
                ImpactSomething();
        }

        public override void Draw()
        {
            base.Draw();
            Matrix4x4 matrix = default;
            matrix.SetTRS(ExactPosition - new Vector3(1, 0,0), PullVelocity.AngleFlat().ToQuat(), Vector3.one * PullStrength);
            Graphics.DrawMesh(MeshPool.plane10, matrix, TiberiumContent.ArrowMat, 0);

            Matrix4x4 matrix2 = default;
            matrix2.SetTRS(ExactPosition + new Vector3(1, 0, 0), PushVelocity.AngleFlat().ToQuat(), Vector3.one * PushStrength);
            Graphics.DrawMesh(MeshPool.plane10, matrix2, TiberiumContent.ArrowMat, 0);
        }

        //Velocities
        //Push Away
        private float PushStrength => Mathf.Clamp01(1f - DistanceCoveredFraction);
        private Vector3 PushDirection => usedTarget.CenterVector3 - ExactPosition;

        private Vector3 PushVelocity
        {
            get
            {
                var osci = TMath.OscillateBetween(-WiggleValue, WiggleValue, OSC_TICKS, startTick + tickOffset) * Mathf.Clamp01(1f - (DistanceCoveredFraction * 2));
                return (Vector3.Scale((((PushDirection.normalized + new Vector3(osci, 0, 0)) * speed) * 0.75f), new Vector3(8 * (Mathf.Clamp01(DistanceCoveredFraction * 3)), 1, 1))) * PushStrength;
            }
        }

        //Pull To
        private float PullStrength => Mathf.Clamp01(DistanceCoveredFraction);
        private Vector3 PullDirection => intendedTarget.CenterVector3 - ExactPosition;
        private Vector3 PullVelocity => ((PullDirection.normalized * speed) * (2f + (6f * PullStrength))) * PullStrength;

        //Data
        private IntVec3 ActualPosition => ExactPosition.ToIntVec3();
        public override Quaternion ExactRotation => (PushVelocity + PullVelocity).AngleFlat().ToQuat();
        public override Vector3 ExactPosition => exactPos ?? base.ExactPosition;
        public override Vector3 DrawPos => ExactPosition;

        public override bool PreLaunch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            actualTarget = intendedTarget.Cell;
            offsetTarget = intendedTarget.Cell + new IntVec3(0, 0, 5);

            int sign = TRandom.Chance(0.5f) ? -1 : 1;
            offsetTarget = offsetTarget.Cell + (new IntVec3(sign * 7, 0,0));
            return true;
        }

        public override void PostLaunch(ref Vector3 origin, ref Vector3 destination)
        {
        }

        public override void CanHitOverride(Thing thing, ref bool result)
        {
        }

        public override bool PreImpact()
        {
            return true;
        }

        public override void PostImpact()
        {
        }
    }
}
