using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
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
