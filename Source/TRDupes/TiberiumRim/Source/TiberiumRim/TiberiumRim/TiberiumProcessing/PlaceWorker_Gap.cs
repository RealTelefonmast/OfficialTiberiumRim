using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class PlaceWorker_Gap : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if(!DebugSettings.godMode && GenAdj.CellsOccupiedBy(loc, rot, def.Size + IntVec2.Two).Any(c => HasDuplicate(c, map, def)))
            {
                return "TR_PW_Gap".Translate();
            }
            return true;
        }

        private bool HasDuplicate(IntVec3 pos, Map map, BuildableDef def)
        {
            var blueprint = pos.GetFirstThing(map, def.blueprintDef);
            var thing = pos.GetFirstThing(map, (ThingDef)def);
            return (blueprint?.Spawned ?? false) || (thing?.Spawned ?? false);
        }
    }
}
