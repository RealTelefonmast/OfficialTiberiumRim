using System.Collections.Generic;
using Verse;

namespace TR;

public class RepairDroneKindDef : MechanicalPawnKindDef
{
    public float healFloat = 0.01f;
}

public class RepairDrone : MechanicalPawn, IPawnWithParent
{
    public Comp_DroneStation parentComp;
    public new RepairDroneKindDef kindDef => base.kindDef as RepairDroneKindDef;

    public bool OutsideOfStationRadius => parentComp.parent.Position.DistanceTo(Position) > parentComp.Props.radius;

    public List<IntVec3> Field => null;
    public ThingWithComps Parent => parent;

    public bool CanWander
    {
        get
        {
            if (!CurJob?.targetA.HasThing ?? true)
                return true;
            return CurJob.targetA.Thing == null;
        }
    }

    public List<IntVec3> Field => null;
    public ThingWithComps Parent => parent;

    public bool CanWander
    {
        get
        {
            if (!CurJob?.targetA.HasThing ?? true)
                return true;
            return CurJob.targetA.Thing == null;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        parentComp = parent?.GetComp<Comp_DroneStation>();
        if (parent == null)
            Log.Warning("RepairDrone Spawned without parent");
        base.SpawnSetup(map, respawningAfterLoad);
    }

    public override void PostMake()
    {
        base.PostMake();
    }

    public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
    {
        DeSpawn();
    }
}

}