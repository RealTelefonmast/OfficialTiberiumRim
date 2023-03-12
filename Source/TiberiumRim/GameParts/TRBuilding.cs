using System.Collections.Generic;
using RimWorld;
using TeleCore.RWExtended;
using Verse;

namespace TiberiumRim
{
    public class TRBuilding : TeleBuilding
    {
        public new TRThingDef def => (TRThingDef) base.def;
        

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            TiberiumRimComp.SuperWeaponInfo.TryRegisterSuperweapon(this);
            TiberiumComp.RegisterTRBuilding(this);
            foreach (IntVec3 c in this.OccupiedRect())
            {
                c.GetPlant(Map)?.DeSpawn();
                if (def.clearTiberium)
                    c.GetTiberium(Map)?.DeSpawn();
                if (def.makesTerrain != null)
                    map.terrainGrid.SetTerrain(c, def.makesTerrain);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            TiberiumComp.DeregisterTRBuilding(this);
            var thingToLeave = def.leavesThing;

            Map map = MapHeld;
            IntVec3 pos = PositionHeld;
            base.DeSpawn(mode);

            if (thingToLeave != null)
                GenSpawn.Spawn(thingToLeave, pos, map);
        }

        public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
        public MapComponent_Tiberium TiberiumComp => Map.Tiberium();

        public bool CannotHaveDuplicates => def.placeWorkers.Any(p => p == typeof(PlaceWorker_Once));
        
        public static TargetingParameters ForAny()
        {
            return new TargetingParameters
            {
                canTargetLocations = true,
                canTargetBuildings = false,
                canTargetFires = false,
                canTargetItems = false,
                canTargetPawns = false,
                canTargetSelf = false,
                validator = t => t.Cell.InBounds(Find.CurrentMap)
            };
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            //
            //if(!def.devObject)
            //yield return new Designator_BuildFixed(def);

            if (def.superWeapon?.ResolvedDesignator != null)
                yield return def.superWeapon.ResolvedDesignator;
        }
    }
}
