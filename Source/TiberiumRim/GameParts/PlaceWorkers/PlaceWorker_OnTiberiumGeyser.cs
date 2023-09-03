using Verse;

namespace TR
{
    public class PlaceWorker_OnTiberiumGeyser : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing nullThing = null)
        {
            Thing thing = map.thingGrid.ThingAt(loc, TiberiumDefOf.TiberiumGeyser);
            if (thing == null || thing.Position != loc)
            {
                return "TR_OnGeyser".Translate();
            }
            return true;
        }
    }
}
