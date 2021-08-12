using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class TRThing : FXThing, IDiscoverable
    {
        public new TRThingDef def;

        public WorldComponent_TR TiberiumRimComp => Find.World.GetComponent<WorldComponent_TR>();
        public MapComponent_Tiberium TiberiumMapComp => Map.Tiberium();

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
            def = (TRThingDef)base.def;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
        }

        public override string GetInspectString()
        {
            string str = (IsDiscoverable && !Discovered) ? "TR_NotDiscovered".Translate().ToString() + "\n" : "";
            str += base.GetInspectString();
            return str;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

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
