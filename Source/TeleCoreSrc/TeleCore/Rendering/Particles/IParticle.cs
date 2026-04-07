using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Particles;

public interface IParticle
{
    public bool ShouldMove { get; }
    public bool ShouldFinalize { get; }
    public Vector3 ExactPos { get; }
    public IntVec3 CellPos { get; }
    public Vector3 DirectionVector { get; set; }
    public void DoInitEvent();
    public void DoFinalEvent();
}