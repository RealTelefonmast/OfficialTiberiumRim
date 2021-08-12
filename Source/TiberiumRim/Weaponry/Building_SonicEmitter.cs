using UnityEngine;

namespace TiberiumRim
{
    public class Building_SonicEmitter : Building_TRTurret
    {
        public override ExtendedGraphicData ExtraData => (def as FXThingDef).extraData;

        public override Vector3[] DrawPositions => new Vector3[] { base.DrawPos};
        public override Color[] ColorOverrides => new Color[] { Color.white};
        public override float[] OpacityFloats => new float[] { 1f };
        public override float?[] RotationOverrides => new float?[] { MainGun.TurretRotation };
        public override bool[] DrawBools => new bool[] { true};
        public override bool ShouldDoEffecters => true;
    }
}
