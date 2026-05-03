using System.Collections.Generic;
using TeleCore.Comps;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public class TeleThing : FXThing, IDiscoverable
{
    public override string Label => Discovered ? DiscoveredLabel : UnknownLabel;
    public override string DescriptionFlavor => Discovered ? DiscoveredDescription : UnknownDescription;

    public TeleDefExtension Extension { get; private set; }
    public DiscoveryProperties Discovery => Extension?.discovery!;

    public bool IsDiscoverable => Discovery != null;

    public DiscoveryDef DiscoveryDef => Discovery.discoveryDef;
    public string DiscoveredLabel => base.Label;
    public string UnknownLabel => Discovery.UnknownLabelCap;
    public string DiscoveredDescription => def.description;
    public string UnknownDescription => Discovery.unknownDescription;
    public string DescriptionExtra => Discovery.extraDescription;
    public bool Discovered => !IsDiscoverable || TFind.Discoveries[this];

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (def.HasTeleExtension(out var textension))
        {
            Extension = textension;
            if (Extension.addCustomTick)
                TeleEventHandler.EntityTicked += TeleTick;
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
        if (Extension != null)
            if (Extension.addCustomTick)
                TeleEventHandler.EntityTicked -= TeleTick;
    }

    protected virtual void TeleTick()
    {
        foreach (var comp in AllComps)
            if (comp is TeleComp teleComp)
                teleComp.TeleTick();
    }

    public override string GetInspectString()
    {
        var str = IsDiscoverable && !Discovered ? "TELE.Discovery.NotDiscovered".Translate().ToString() + "\n" : "";
        str += base.GetInspectString();
        return str;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;

        if (!DebugSettings.godMode) yield break;
        if (IsDiscoverable && !Discovered)
            yield return new Command_Action
            {
                defaultLabel = "Discover",
                action = delegate { DiscoveryDef.Discover(); }
            };
    }
}