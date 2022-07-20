using System.Collections.Generic;
using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class HediffDiscoverable : HediffWithGizmos, IDiscoverable
    {
        public new TRHediffDef def;

        public override string Label => Discovered ? DiscoveredLabel : UnknownLabel;

        //public override string DescriptionFlavor => Discovered ? DiscoveredDescription : UnknownDescription;

        public DiscoveryDef DiscoveryDef => def.discovery.discoveryDef;
        public string DiscoveredLabel => base.Label;
        public string UnknownLabel => def.UnknownLabelCap;
        public string DiscoveredDescription => def.description;
        public string UnknownDescription => def.discovery.unknownDescription;
        public string DescriptionExtra => def.discovery.extraDescription;

        public bool Discovered => !IsDiscoverable || TRUtils.DiscoveryTable().IsDiscovered(this);
        public bool IsDiscoverable => def.discovery != null;

        /*
        public ove string GetInspectString()
        {
            string str = (IsDiscoverable && !Discovered) ? "TR_NotDiscovered".Translate().ToString() + "\n" : "";
            str += base.GetInspectString();
            return str;
        }
        */

        public override IEnumerable<Gizmo> GetGizmos()
        {
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
