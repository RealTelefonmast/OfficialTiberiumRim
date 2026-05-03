using Verse;

namespace TeleCore.PlaceWorkers;

public class PlaceWorker_PlaceOverBuilding : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        if (loc.GetEdifice(map) == null)
            return "TELE.PlaceOverBuilding.OnBuilding".Translate();
        return true;
    }

    public override bool ForceAllowPlaceOver(BuildableDef other)
    {
        return other.IsEdifice();
    }
}