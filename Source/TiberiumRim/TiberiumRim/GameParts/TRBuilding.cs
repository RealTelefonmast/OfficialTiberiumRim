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
        public new TRThingDef def;

        public override string Label => Discovered ? DiscoveredLabel : UnknownLabel;

        public override string DescriptionFlavor => Discovered ? DiscoveredDescription : UnknownDescription;

        public string DiscoverTag => def.discoverTag;
        public string DiscoveredLabel => base.Label;
        public string UnknownLabel => def.UnknownLabelCap;
        public string DiscoveredDescription => def.description;
        public string UnknownDescription => def.unknownDescription;
        public string DescriptionExtra => def.extraDescription;

        public bool Discovered => !IsDiscoverable || TRUtils.DiscoveryTable().IsDiscovered(this);
        public bool IsDiscoverable => DiscoverTag != null;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.def = (TRThingDef)base.def;
            TiberiumRimComp.TryRegisterSuperweapon(this);
            TiberiumComp.StructureInfo.TryRegister(this);
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
            TiberiumComp.StructureInfo.Deregister(this);
            var thingToLeave = def.leavesThing;
            if (thingToLeave != null)
                GenSpawn.Spawn(thingToLeave, this.Position, Map);

            base.DeSpawn(mode);
        }

        public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
        public WorldComponent_Tiberium WorldTiberiumComp => Find.World.GetComponent<WorldComponent_Tiberium>();
        public MapComponent_Tiberium TiberiumComp => Map.GetComponent<MapComponent_Tiberium>();

        public bool CannotHaveDuplicates => def.placeWorkers.Any(p => p == typeof(PlaceWorker_Once));

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            if (IsDiscoverable)
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
                    action = delegate { TRUtils.DiscoveryTable().Discover(DiscoverTag); }
                };
            }

        }
    }
}
