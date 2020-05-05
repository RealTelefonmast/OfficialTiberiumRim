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

        public string DiscoverTag => def.discoverTag;
        public string DiscoveredLabel => base.Label;
        public string UnknownLabel => def.UnknownLabelCap;
        public string DiscoveredDescription => def.description;
        public string UnknownDescription => def.unknownDescription;
        public string DescriptionExtra => def.extraDescription;

        public bool Discovered => !IsDiscoverable || TRUtils.DiscoveryTable().IsDiscovered(this);
        public bool IsDiscoverable => DiscoverTag != null;

        public override string GetInspectString()
        {
            string str = (IsDiscoverable && !Discovered) ? "TR_NotDiscovered".Translate().ToString() : "";
            str += "\n" + base.GetInspectString();
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
                    action = delegate { TRUtils.DiscoveryTable().Discover(DiscoverTag); }
                };
            }
        }
    }
}
