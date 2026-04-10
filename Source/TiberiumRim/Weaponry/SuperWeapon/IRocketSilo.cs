using UnityEngine;
using Verse;

namespace TR.SuperWeapon;

public interface IRocketSilo
{
    Vector3 RocketBaseOffset { get; }
    AltitudeLayer Altitude { get; }
}