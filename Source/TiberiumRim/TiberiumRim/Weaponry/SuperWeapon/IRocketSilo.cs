using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
