using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class RefineryTargetInfo : TargetingParameters
    {
        public static TargetingParameters ForHarvester()
        {
            return new TargetingParameters
            {
                canTargetBuildings = true,
                canTargetFires = false,
                canTargetItems = false,
                canTargetLocations = false,
                canTargetPawns = false,
                canTargetSelf = false,                
                validator = t => t.Thing is Building b && b.TryGetComp<CompTNW_Refinery>() != null                       
            };
        }
    }
}
