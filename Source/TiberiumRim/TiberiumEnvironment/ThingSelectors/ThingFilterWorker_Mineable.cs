using Verse;

namespace TR
{
    public class ThingFilterWorker_Mineable : ThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            return false;
        }

        public override bool Matches(ThingDef tDef)
        {
            return tDef?.building?.isNaturalRock ?? false;
        }
    }
}
