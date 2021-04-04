using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TRBuilding : FXBuilding, IDiscoverable
    {
        public new TRThingDef def => (TRThingDef)base.def;

        public override string Label => Discovered ? DiscoveredLabel : UnknownLabel;

        public override string DescriptionFlavor => Discovered ? DiscoveredDescription : UnknownDescription;

        public DiscoveryDef DiscoveryDef => def.discovery.discoveryDef;
        public string DiscoveredLabel => base.Label;
        public string UnknownLabel => def.UnknownLabelCap;
        public string DiscoveredDescription => def.description;
        public string UnknownDescription => def.discovery.unknownDescription;
        public string DescriptionExtra => def.discovery.extraDescription;

        public bool Discovered => !IsDiscoverable || TRUtils.DiscoveryTable().IsDiscovered(this);
        public bool IsDiscoverable => def.discovery != null;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            TiberiumRimComp.SuperWeaponInfo.TryRegisterSuperweapon(this);
            TiberiumComp.RegisterTiberiumBuilding(this);
            foreach (IntVec3 c in this.OccupiedRect())
            {
                c.GetPlant(Map)?.DeSpawn();
                if (def.clearTiberium) 
                    c.GetTiberium(Map)?.DeSpawn();
                if(def.makesTerrain != null)
                    map.terrainGrid.SetTerrain(c, def.makesTerrain);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            TiberiumComp.DeregisterTiberiumBuilding(this);
            var thingToLeave = def.leavesThing;

            Map map = MapHeld;
            IntVec3 pos = PositionHeld;
            base.DeSpawn(mode);

            if (thingToLeave != null)
                GenSpawn.Spawn(thingToLeave, pos, map);
        }

        public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
        public MapComponent_Tiberium TiberiumComp => Map.GetComponent<MapComponent_Tiberium>();

        public bool CannotHaveDuplicates => def.placeWorkers.Any(p => p == typeof(PlaceWorker_Once));

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            if (IsDiscoverable && !Discovered)
                str += "\n"+"TR_NotDiscovered".Translate();

            return str;
        }

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

            if(!DebugSettings.godMode) yield break;

            if (IsDiscoverable && !Discovered)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Discover",
                    action = delegate { DiscoveryDef.Discover(); }
                };
            }
        }
    }
}
