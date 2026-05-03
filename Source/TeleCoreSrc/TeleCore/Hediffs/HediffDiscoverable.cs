using System.Collections.Generic;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public class HediffDiscoverable : HediffWithGizmos, IDiscoverable
{
    public TeleDefExtension Extension { get; private set; }
    public DiscoveryProperties Discovery => Extension?.discovery!;

    public override string Label => Discovered ? DiscoveredLabel : UnknownLabel;

    public bool IsDiscoverable => Discovery != null;

    //public override string DescriptionFlavor => Discovered ? DiscoveredDescription : UnknownDescription;

    public DiscoveryDef DiscoveryDef => Discovery.discoveryDef;
    public string DiscoveredLabel => base.Label;
    public string UnknownLabel => Discovery.UnknownLabelCap;
    public string DiscoveredDescription => def.description;
    public string UnknownDescription => Discovery.unknownDescription;
    public string DescriptionExtra => Discovery.extraDescription;
    public bool Discovered => !IsDiscoverable || TFind.Discoveries[this];


    public override void PostMake()
    {
        base.PostMake();
        Extension = def.TeleExtension();
    }

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
        if (!DebugSettings.godMode) yield break;
        if (IsDiscoverable && !Discovered)
            yield return new Command_Action
            {
                defaultLabel = "Discover Hediff",
                action = delegate { DiscoveryDef.Discover(); }
            };
    }
}