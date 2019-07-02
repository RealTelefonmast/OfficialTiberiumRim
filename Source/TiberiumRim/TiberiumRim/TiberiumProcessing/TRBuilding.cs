using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TRBuilding : Building
    {
        public new TRThingDef def;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.def = (TRThingDef)base.def;
            if (def.destroyTiberium)
            {
                foreach(IntVec3 c in this.OccupiedRect())
                {
                    c.GetTiberium(Map)?.DeSpawn();
                }
            }
        }
    }
}
