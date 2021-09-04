using UnityEngine;
using Verse;

namespace TiberiumRim
{
    //TODO: Implement IPatchedProjectile
    public class Projectile_Homing : Projectile_Explosive
    {
        private float speed = 1f;
        private Vector3? exactPos;
        private LocalTargetInfo arcTarget;

        public LocalTargetInfo MainTarget => usedTarget;

        public LocalTargetInfo ArcTarget
        {
            get => arcTarget;
            private set => arcTarget = value;
        }

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

        private Vector3? initVector;

        private int startTick = 0;
        public override void Tick()
        {
            startTick++;

            //Get initial directional speed vector
            initVector ??= VelocityAway;
            exactPos ??= origin;

            var val = (DistanceCoveredFraction/3f) * 2;
            var actualVal = Mathf.Lerp(0, val, DistanceCoveredFraction);
            exactPos += ((VelocityTo + VelocityAway) * (0.0166666675f * 1));
            if (ActualPosition == MainTarget.Cell)
                Impact(MainTarget.Thing);

            base.Tick();
        }

        public override void Draw()
        {
            base.Draw();
            Matrix4x4 matrix = default;
            matrix.SetTRS(ExactPosition - new Vector3(1, 0,0), VelocityTo.AngleFlat().ToQuat(), Vector3.one * ToTargetPullStrength);
            Graphics.DrawMesh(MeshPool.plane10, matrix, TiberiumContent.ArrowMat, 0);

            Matrix4x4 matrix2 = default;
            matrix2.SetTRS(ExactPosition + new Vector3(1, 0, 0), VelocityAway.AngleFlat().ToQuat(), Vector3.one * AwayPullStrength);
            Graphics.DrawMesh(MeshPool.plane10, matrix2, TiberiumContent.ArrowMat, 0);
        }

        //Velocities
        //Pull away from target
        private float AwayPullStrength => Mathf.Clamp01(1f - DistanceCoveredFraction);
        private Vector3 DirectionAwayFromTarget => usedTarget.CenterVector3 - ExactPosition;

        //[TweakValue("HOMING_OSC", 0f, 2f)]
        //public static float WiggleValue = 0.5f;

        //[TweakValue("HOMING_OSC_TIME", 1, 100)]
        //public static int OSC_TICKS = 25;

        private int tickOffset = 0;

        private Vector3 VelocityAway
        {
            get
            {
                var osci = 0; //TRUtils.OscillateBetween(-WiggleValue, WiggleValue, OSC_TICKS, startTick + tickOffset) * Mathf.Clamp01(1f - (DistanceCoveredFraction * 2));
                return (Vector3.Scale((((DirectionAwayFromTarget.normalized + new Vector3(osci, 0, 0)) * speed) * 0.75f), new Vector3(8 * (Mathf.Clamp01(DistanceCoveredFraction * 3)), 1, 1))) * AwayPullStrength;
            }
        }

        //Pull to target
        private float ToTargetPullStrength => Mathf.Clamp01(DistanceCoveredFraction);
        private Vector3 DirectionToTarget => intendedTarget.CenterVector3 - ExactPosition;
        private Vector3 VelocityTo => ((DirectionToTarget.normalized * speed) * (2f + (6f * ToTargetPullStrength))) * ToTargetPullStrength;

        //Data
        private IntVec3 ActualPosition => ExactPosition.ToIntVec3();
        public override Quaternion ExactRotation => (VelocityAway + VelocityTo).AngleFlat().ToQuat();
        public override Vector3 ExactPosition => exactPos ?? base.ExactPosition;
        public override Vector3 DrawPos => ExactPosition;
    }
}
