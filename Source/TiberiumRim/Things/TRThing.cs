using System.Collections.Generic;
using TiberiumRim;
using Verse;
using MapComponent_Tiberium = TR.Components.MapComponent_Tiberium;
using WorldComponent_TR = TR.World.WorldComponent_TR;

namespace TR;

public class TRThing : TeleThing
{
    public new TRThingDef def;

    public WorldComponent_TR TiberiumRimComp => Find.World.GetComponent<WorldComponent_TR>();
    public MapComponent_Tiberium TiberiumMapComp => Map.GetComponent<MapComponent_Tiberium>();

    public override string Label => Discovered ? DiscoveredLabel : UnknownLabel;
    public override string DescriptionFlavor => Discovered ? DiscoveredDescription : UnknownDescription;
    public new bool IsDiscoverable => def.discovery != null;

    public new DiscoveryDef DiscoveryDef => def.discovery.discoveryDef;
    public string DiscoveredLabel => base.Label;
    public new string UnknownLabel => def.UnknownLabelCap;
    public string DiscoveredDescription => def.description;
    public new string UnknownDescription => def.discovery.unknownDescription;
    public new string DescriptionExtra => def.discovery.extraDescription;

    public new bool Discovered => !IsDiscoverable || TRUtils.DiscoveryTable().IsDiscovered(this);

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
        var str = IsDiscoverable && !Discovered ? "TR_NotDiscovered".Translate().ToString() + "\n" : "";
        str += base.GetInspectString();
        return str;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;

        if (IsDiscoverable && !Discovered)
            yield return new Command_Action
            {
                defaultLabel = "Discover",
                action = delegate { DiscoveryDef.Discover(); }
            };
    }
}