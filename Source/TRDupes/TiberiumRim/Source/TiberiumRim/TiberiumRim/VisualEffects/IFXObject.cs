using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public interface IFXObject
    {
        ExtendedGraphicData ExtraData { get; }
        float[] OpacityFloats { get; }
        float?[] RotationOverrides { get; }
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
