using System.Collections.Generic;
using TeleCore.RWExtended;
using Verse;

namespace TR
{
    public class TRThing : TeleThing
    {
        public new TRThingDef def;

        public WorldComponent_TR TiberiumRimComp => Find.World.GetComponent<WorldComponent_TR>();
        public MapComponent_Tiberium TiberiumMapComp => MapHeld.Tiberium();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = (TRThingDef)base.def;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
        }
        

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
        }
    }
}
