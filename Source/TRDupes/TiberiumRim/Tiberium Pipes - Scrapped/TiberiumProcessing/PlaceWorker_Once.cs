using System.Collections.Generic;
using System.Collections;
using Verse;
using RimWorld;
using System.Linq;

namespace TiberiumRim
{
    public class PlaceWorker_Once : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            var num = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(checkingDef as ThingDef).Count();
            if (num > 0)
            {
                return "TR_ThingAlreadyExists".Translate(checkingDef.LabelCap);
            }
            return true;
        }
    }
}
