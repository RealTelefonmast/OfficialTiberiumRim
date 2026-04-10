using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class TiberiumTargetingParameters : TargetingParameters
    {
        public static TargetingParameters ForRefinery()
        {
            return new TargetingParameters
            {
                canTargetBuildings = true,
                canTargetPawns = false,
                canTargetSelf = false,
                canTargetItems = false,
                canTargetFires = false,
                canTargetLocations = false
            };
        }
    }
}
