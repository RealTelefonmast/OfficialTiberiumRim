using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class TiberiumThing : FXThing
    {
        public new TRThingDef def;

        public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
        public WorldComponent_Tiberium WorldTiberiumComp => Find.World.GetComponent<WorldComponent_Tiberium>();
        public MapComponent_Tiberium TiberiumComp => Map.GetComponent<MapComponent_Tiberium>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = (TRThingDef)base.def;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
        }
    }
}
