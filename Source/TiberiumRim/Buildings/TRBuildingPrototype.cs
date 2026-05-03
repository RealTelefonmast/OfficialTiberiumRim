using System.Collections.Generic;
using RimWorld;
using TiberiumRim;
using Verse;

namespace TR;

public class TRBuildingPrototype : TeleBuilding
{
    public new TRThingDef def => (TRThingDef)base.def;

    public bool CannotHaveDuplicates => def.placeWorkers.Any(p => p == typeof(PlaceWorker_Once));


    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        TRUtils.Tiberium().Notify_BuildingSpawned(this);
        foreach (var c in this.OccupiedRect())
        {
            c.GetPlant(Map)?.DeSpawn();
            if (def.makesTerrain != null)
                map.terrainGrid.SetTerrain(c, def.makesTerrain);
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        var thingToLeave = def.leavesThing;

        var map = MapHeld;
        var pos = PositionHeld;
        base.DeSpawn(mode);

        if (thingToLeave != null)
            GenSpawn.Spawn(thingToLeave, pos, map);
    }

    public static TargetingParameters ForAny()
    {
        return new TargetingParameters
        {
            canTargetLocations = true,
            canTargetBuildings = false,
            canTargetFires = false,
            canTargetItems = false,
            canTargetPawns = false,
            canTargetSelf = false,
            validator = t => t.Cell.InBounds(Find.CurrentMap)
        };
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;

        //
        //if(!def.devObject)
        //yield return new Designator_BuildFixed(def);

        if (def.superWeapon?.ResolvedDesignator != null)
            yield return def.superWeapon.ResolvedDesignator;
    }
}