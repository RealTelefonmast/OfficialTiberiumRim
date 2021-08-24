using System;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public interface IFXObject
    {
        ExtendedGraphicData ExtraData { get; }
        float[] OpacityFloats { get; }
        float?[] RotationOverrides { get; }
        float?[] AnimationSpeeds { get; }
        bool[] DrawBools { get; }
        Color[] ColorOverrides { get; }
        Vector3[] DrawPositions { get; }
        Action<FXGraphic>[] Actions { get; }
        Vector2? TextureOffset { get; }
        Vector2? TextureScale { get; }
        bool ShouldDoEffecters  { get; }
        CompPower ForcedPowerComp { get; }
    }
}
