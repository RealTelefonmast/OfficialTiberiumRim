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

        public TiberiumGarden parentGarden;

        public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
        public WorldComponent_Tiberium WorldTiberiumComp => Find.World.GetComponent<WorldComponent_Tiberium>();
        public MapComponent_Tiberium TiberiumMapComp => Map.GetComponent<MapComponent_Tiberium>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = (TRThingDef)base.def;
            TiberiumMapComp.RegisterTiberiumPlant(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {

            TiberiumMapComp.DeregisterTiberiumPlant(this);
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
