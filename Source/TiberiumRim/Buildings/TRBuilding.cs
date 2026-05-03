using System.Collections.Generic;
using TR.Designators;
using Verse;

namespace TR;

public class TRBuilding : FXBuilding, IDiscoverable
{
    public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();

    // B — canonical (field set in SpawnSetup; A used an expression property)
    public new TRThingDef def;

    // A — conditional label; B simplified to DiscoveredLabel only (A's is more correct)
    public override string Label => Discovered ? DiscoveredLabel : UnknownLabel;

    public override string DescriptionFlavor => Discovered ? DiscoveredDescription : UnknownDescription;

    // B — unique
    public WorldComponent_Tiberium WorldTiberiumComp => Find.World.GetComponent<WorldComponent_Tiberium>();
    public MapComponent_Tiberium TiberiumComp => Map.GetComponent<MapComponent_Tiberium>();

    public bool CannotHaveDuplicates => def.placeWorkers.Any(p => p == typeof(PlaceWorker_Once));

    // B — newer discovery API (tag-based rather than def.discovery object)
    public string DiscoverTag => def.discoverTag;
    public bool Discovered => DiscoverTag.NullOrEmpty() || TRUtils.DiscoveryTable().IsDiscovered(this);

    // A — keeps IsDiscoverable as a named concept
    public bool IsDiscoverable => def.discovery != null;

    // B canonical for DiscoveredLabel (LabelCap); A used base.Label
    public string DiscoveredLabel => base.LabelCap;
    public string UnknownLabel => def.UnknownLabelCap;
    public string DiscoveredDescription => def.description;

    // B canonical — flat def fields; A routed through def.discovery object
    public string UnknownDescription => def.unknownDescription;
    public string DescriptionExtra => def.extraDescription;

    // A — DiscoveryDef through discovery object (still useful until discovery is fully migrated)
    public DiscoveryDef DiscoveryDef => def.discovery.discoveryDef;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        def = (TRThingDef)base.def;                         // B
        TiberiumRimComp.TryRegisterSuperweapon(this);       // B canonical (A used .SuperWeaponInfo.)
        TiberiumComp.StructureInfo.TryRegister(this);       // B canonical (A used .RegisterTiberiumBuilding)
        foreach (var c in this.OccupiedRect())
        {
            c.GetPlant(Map)?.DeSpawn();
            if (def.destroyTiberium)                        // B canonical (A used clearTiberium)
                c.GetTiberium(Map)?.DeSpawn();
            if (def.makesTerrain != null)
                map.terrainGrid.SetTerrain(c, def.makesTerrain);
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        TiberiumComp.StructureInfo.Deregister(this);        // B canonical (A used .DeregisterTiberiumBuilding)

        // A — leavesThing spawn after despawn (unique, not in B)
        var thingToLeave = def.leavesThing;
        var map = MapHeld;
        var pos = PositionHeld;

        base.DeSpawn(mode);

        if (thingToLeave != null)
            GenSpawn.Spawn(thingToLeave, pos, map);
    }

    // A — unique (B removed this entirely)
    public override string GetInspectString()
    {
        var str = base.GetInspectString();
        if (IsDiscoverable && !Discovered)
            str += "\n" + "TR_NotDiscovered".Translate();

        return str;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;

        if (!def.devObject)
            yield return new Designator_BuildFixed(def);

        if (def.superWeapon?.ResolvedDesignator != null)
            yield return def.superWeapon.ResolvedDesignator;

        // A — debug discover gizmo (B removed this)
        if (!DebugSettings.godMode) yield break;

        if (IsDiscoverable && !Discovered)
            yield return new Command_Action
            {
                defaultLabel = "Discover",
                action = delegate { DiscoveryDef.Discover(); }
            };
    }
}

