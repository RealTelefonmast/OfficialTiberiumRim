using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class TRThing : FXThing
    {
        public new TRThingDef def;

        public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
        public MapComponent_Tiberium TiberiumMapComp => Map.GetComponent<MapComponent_Tiberium>();


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = (TRThingDef)base.def;
            TiberiumMapComp.RegisterTiberiumThing(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            TiberiumMapComp.DeregisterTiberiumThing(this);
            base.DeSpawn(mode);
        }
    }
}
