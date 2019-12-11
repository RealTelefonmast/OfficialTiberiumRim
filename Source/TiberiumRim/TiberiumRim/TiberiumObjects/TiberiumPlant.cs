using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumPlant : Plant
    {
        public new TRThingDef def;

        public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
        public WorldComponent_Tiberium WorldTiberiumComp => Find.World.GetComponent<WorldComponent_Tiberium>();
        public MapComponent_Tiberium TiberiumComp => Map.GetComponent<MapComponent_Tiberium>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = (TRThingDef)base.def;
            TiberiumComp.AddTiberiumPlant(this, respawningAfterLoad);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            TiberiumComp.RemoveTiberiumPlant(this);
            base.DeSpawn(mode);
        }

        public override bool BlightableNow => false;
        public override bool IngestibleNow => false;

        public override float CurrentDyingDamagePerTick
        {
            get
            {
                return base.CurrentDyingDamagePerTick;
            }
        }
    }
}
