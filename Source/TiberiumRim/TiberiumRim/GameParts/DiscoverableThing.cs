using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class DiscoverableThing : TiberiumThing, IDiscoverable
    {
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
                    action = delegate { TRUtils.DiscoveryTable().Discover(DiscoveryDef); }
                };
            }
        }
    }
}
