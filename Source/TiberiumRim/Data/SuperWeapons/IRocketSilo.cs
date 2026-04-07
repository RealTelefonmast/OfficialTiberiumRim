using UnityEngine;
using Verse;

namespace TR.Data.SuperWeapons
{
    public interface IRocketSilo
    {
        Vector3 RocketBaseOffset { get; }
        AltitudeLayer Altitude { get; }


    }
}
