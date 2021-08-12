using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public interface IRocketSilo
    {
        Vector3 RocketBaseOffset { get; }
        AltitudeLayer Altitude { get; }


    }
}
