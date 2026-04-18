using System.Collections.Generic;
using TeleCore.Comps;
using TeleCore.Defs;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Buildings;

public class TeleBuilding : FXBuilding, IDiscoverable
{
    public override string Label => Discovered ? DiscoveredLabel : UnknownLabel;
    public override string DescriptionFlavor => Discovered ? DiscoveredDescription : UnknownDescription;

    public TeleDefExtension? TeleExtension { get; private set; }
    public DiscoveryProperties? Discovery => TeleExtension?.discovery!;

    public bool IsDiscoverable => Discovery != null;

    public DiscoveryDef DiscoveryDef => Discovery!.discoveryDef;
    public string DiscoveredLabel => base.Label;
    public string UnknownLabel => Discovery!.UnknownLabelCap;
    public string DiscoveredDescription => def.description;
    public string UnknownDescription => Discovery!.unknownDescription;
    public string DescriptionExtra => Discovery!.extraDescription;
    public bool Discovered => !IsDiscoverable || TFind.Discoveries[this];

    public override void SpawnSetup(Verse.Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (def.HasTeleExtension(out var textension))
        {
            TeleExtension = textension;
            if (TeleExtension.addCustomTick)
                TeleEventHandler.EntityTicked += TeleTickInternal;
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
        if (TeleExtension != null)
            if (TeleExtension.addCustomTick)
                TeleEventHandler.EntityTicked -= TeleTickInternal;
    }

    private void TeleTickInternal()
    {
        foreach (var comp in AllComps)
            if (comp is TeleComp teleComp)
                teleComp.TeleTick();

        //
        TeleTick();
    }

    /// <summary>
    /// Ticks after all comps have ticked.
    /// </summary>
    protected virtual void TeleTick()
    {
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