using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TRBuilding : FXBuilding
    {
        public new TRThingDef def;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.def = (TRThingDef)base.def;
            TiberiumComp.StructureInfo.TryRegister(this);
            foreach (IntVec3 c in this.OccupiedRect())
            {
                c.GetPlant(Map)?.DeSpawn();
                if (def.destroyTiberium) 
                    c.GetTiberium(Map)?.DeSpawn();
                if(def.makesTerrain != null)
                    map.terrainGrid.SetTerrain(c, def.makesTerrain);
            }
        }


        public WorldComponent_Tiberium WorldTiberiumComp => Find.World.GetComponent<WorldComponent_Tiberium>();
        public MapComponent_Tiberium TiberiumComp => Map.GetComponent<MapComponent_Tiberium>();
    }
}
