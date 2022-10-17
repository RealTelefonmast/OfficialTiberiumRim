using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class PlaceWorker_WallLight : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (!loc.GetThingList(map).Any(t => t.def.IsWall())) return false;
            IntVec3 newLoc = loc;
            if (rot == Rot4.North)
                newLoc += IntVec3.South;
            else if(rot == Rot4.East)
                newLoc += IntVec3.East;
            else if (rot == Rot4.South)
                newLoc += IntVec3.North;
            else if (rot == Rot4.West)
                newLoc += IntVec3.West;
            return !newLoc.GetThingList(map).Any(t => t.def.IsWall());
        }

        public override bool ForceAllowPlaceOver(BuildableDef other)
        {
            return (other as ThingDef).IsWall();
        }
    }
}
