using UnityEngine;
using Verse;

namespace TR
{
    public interface IRocketSilo
    {
        Vector3 RocketBaseOffset { get; }
        AltitudeLayer Altitude { get; }


    }
}
