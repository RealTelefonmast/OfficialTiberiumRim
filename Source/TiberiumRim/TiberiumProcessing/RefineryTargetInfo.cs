using RimWorld;
using TR.GameParts.Networks.TiberiumNetwork;
using Verse;

namespace TR.TiberiumProcessing
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
                validator = t => t.Thing is Building b && b.TryGetComp<CompTNS_Refinery>() != null                       
            };
        }

        public static TargetingParameters ForTiberiumProducers()
        {
            return new TargetingParameters
            {
                canTargetBuildings = true,
                canTargetFires = false,
                canTargetItems = false,
                canTargetLocations = false,
                canTargetPawns = false,
                canTargetSelf = false,
                validator = t => t.Thing is Data.ThingClasses.TibProducers.TiberiumProducer
            };
        }
    }
}