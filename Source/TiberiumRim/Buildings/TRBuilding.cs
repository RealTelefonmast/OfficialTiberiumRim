using System.Collections.Generic;
using TiberiumRim;
using TR.Designators;
using Verse;
using MapComponent_Tiberium = TR.Components.MapComponent_Tiberium;
using WorldComponent_TR = TR.World.WorldComponent_TR;

namespace TR;

public class TRBuilding : FXBuilding, IDiscoverable
{
    public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
    public new TRThingDef def => (TRThingDef)base.def;

    public override string Label => Discovered ? DiscoveredLabel : UnknownLabel;

    public override string DescriptionFlavor => Discovered ? DiscoveredDescription : UnknownDescription;
    public bool IsDiscoverable => def.discovery != null;
    public MapComponent_Tiberium TiberiumComp => Map.GetComponent<MapComponent_Tiberium>();

    public bool CannotHaveDuplicates => def.placeWorkers.Any(p => p == typeof(PlaceWorker_Once));

    public DiscoveryDef DiscoveryDef => def.discovery.discoveryDef;
    public string DiscoveredLabel => base.Label;
    public string UnknownLabel => def.UnknownLabelCap;
    public string DiscoveredDescription => def.description;
    public string UnknownDescription => def.discovery.unknownDescription;
    public string DescriptionExtra => def.discovery.extraDescription;

    public bool Discovered => !IsDiscoverable || TRUtils.DiscoveryTable().IsDiscovered(this);

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        TiberiumRimComp.SuperWeaponInfo.TryRegisterSuperweapon(this);
        TiberiumComp.RegisterTiberiumBuilding(this);
        foreach (var c in this.OccupiedRect())
        {
            c.GetPlant(Map)?.DeSpawn();
            if (def.clearTiberium)
                c.GetTiberium(Map)?.DeSpawn();
            if (def.makesTerrain != null)
                map.terrainGrid.SetTerrain(c, def.makesTerrain);
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        TiberiumComp.DeregisterTiberiumBuilding(this);
        var thingToLeave = def.leavesThing;

        var map = MapHeld;
        var pos = PositionHeld;
        base.DeSpawn(mode);

        if (thingToLeave != null)
            GenSpawn.Spawn(thingToLeave, pos, map);
    }

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

        if (!DebugSettings.godMode) yield break;

        if (IsDiscoverable && !Discovered)
            yield return new Command_Action
            {
                defaultLabel = "Discover",
                action = delegate { DiscoveryDef.Discover(); }
            };
    }
}
/* OLD OLD REF
 using System.Collections.Generic;
using RimWorld;
using TeleCore.RWExtended;
using Verse;

namespace TR;

public class TRBuilding : TRBuildingPrototype
{
    public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
    public MapComponent_Tiberium TiberiumComp => Map.Tiberium();

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        TiberiumRimComp.SuperWeaponInfo.TryRegisterSuperweapon(this);
        TiberiumComp.RegisterTRBuilding(this);
        foreach (IntVec3 c in this.OccupiedRect())
        {
            if (def.clearTiberium)
                c.GetTiberium(Map)?.DeSpawn();
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        TiberiumComp.DeregisterTRBuilding(this);
        base.DeSpawn(mode);
    }
}

/* OLD REF
using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class TRBuilding : FXBuilding, IDiscoverable
    {
        public new TRThingDef def => (TRThingDef)base.def;

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
            TiberiumRimComp.SuperWeaponInfo.TryRegisterSuperweapon(this);
            TiberiumComp.RegisterTiberiumBuilding(this);
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
            TiberiumComp.DeregisterTiberiumBuilding(this);
            var thingToLeave = def.leavesThing;

            Map map = MapHeld;
            IntVec3 pos = PositionHeld;
            base.DeSpawn(mode);

            if (thingToLeave != null)
                GenSpawn.Spawn(thingToLeave, pos, map);
        }

        public WorldComponent_TR TiberiumRimComp = Find.World.GetComponent<WorldComponent_TR>();
        public MapComponent_Tiberium TiberiumComp => Map.Tiberium();

        public bool CannotHaveDuplicates => def.placeWorkers.Any(p => p == typeof(PlaceWorker_Once));

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            if (IsDiscoverable && !Discovered)
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
                    action = delegate { DiscoveryDef.Discover(); }
                };
            }
        }
    }
}
* /
*/