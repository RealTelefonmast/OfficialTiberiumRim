using System.Linq;
using Verse;

namespace TeleCore.PlaceWorkers;

public class PlaceWorker_Once : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Verse.Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        var num = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(checkingDef as ThingDef).Count();
        if (num > 0)
        {
            return "TELE.ThingAlreadyExists".Translate(checkingDef.LabelCap);
        }

        return true;
    }
}