using Verse;

namespace TR
{
    public class PlaceWorker_OnlyOutside : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (loc.GetRoom(map)?.PsychologicallyOutdoors ?? false)
            {
                return true;
            }
            return "TR_NeedsOutdoors".Translate();
        }
    }
}
