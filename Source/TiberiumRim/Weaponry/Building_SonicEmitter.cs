using TeleCore.Rendering;
using UnityEngine;

namespace TR.Weaponry;

public class Building_SonicEmitter : Building_TRTurret
{
    public override ExtendedGraphicData ExtraData => def.extraData;

    public override Vector3[] DrawPositions => new[] { base.DrawPos };
    public override Color[] ColorOverrides => new[] { Color.white };
    public override float[] OpacityFloats => new[] { 1f };
    public override float?[] RotationOverrides => new float?[] { MainGun.TurretRotation };
    public override bool[] DrawBools => new[] { true };
    public override bool ShouldDoEffecters => true;
}