using RimWorld;
using Verse;

namespace TR
{
    public class PlaceWorker_DoorFrame : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (loc.GetDoor(map) == null)
                return "TR_MustPlaceOnDoor".Translate();
            return true;
        }

        public override bool ForceAllowPlaceOver(BuildableDef other)
        {
            return (other as ThingDef).thingClass.IsAssignableFrom(typeof(Building_Door));
        }
    }
}
