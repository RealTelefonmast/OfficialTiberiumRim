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
            TiberiumRimComp.TryRegisterSuperweapon(this);
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

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            TiberiumComp.StructureInfo.Deregister(this);
            base.DeSpawn(mode);
        }

        public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
        public WorldComponent_Tiberium WorldTiberiumComp => Find.World.GetComponent<WorldComponent_Tiberium>();
        public MapComponent_Tiberium TiberiumComp => Map.GetComponent<MapComponent_Tiberium>();

        public bool CannotHaveDuplicates => def.placeWorkers.Any(p => p == typeof(PlaceWorker_Once));

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if(!def.devObject)
                yield return new Designator_BuildFixed(def);

            if (def.superWeapon?.ResolvedDesignator != null)
                yield return def.superWeapon.ResolvedDesignator;
        }
    }
}
