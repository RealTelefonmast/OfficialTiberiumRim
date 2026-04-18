using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using TiberiumRim;
using UnityEngine;
using Verse;

namespace TR;

public class Comp_RepairDrone : Comp_MechStation
{
    public int radialCells;

    public new CompProperties_RepairDrone Props => props as CompProperties_RepairDrone;

    public ThingOwner DroneContainer => GetDirectlyHeldThings();

    public bool IsPowered
    {
        get
        {
            var comp = parent.TryGetComp<CompPowerTrader>();
            return comp?.PowerOn ?? true;
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        radialCells = GenRadial.NumCellsInRadius(Props.radius);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
    }

    public override void CompTick()
    {
        base.CompTick();
        if (IsPowered) TryReleaseDrone();
    }

    public void TryReleaseDrone()
    {
        var mechs = MechsAvailableForRepair().ToList();
        if (!Enumerable.Any(mechs)) return;
        foreach (var drone in storedMechs)
        {
            if (!DroneContainer.Contains(drone)) continue;
            foreach (var mech in mechs)
            {
                if (parent.Map.physicalInteractionReservationManager.IsReserved(mech)) continue;
                var closestPos = GenAdjFast.AdjacentCells8Way(parent).MinBy(c => c.DistanceTo(mech.Position));
                DroneContainer.TryDrop(drone, closestPos, parent.Map, ThingPlaceMode.Direct, out var last);
                var job = new JobWithExtras(DefDatabase<JobDef>.GetNamed("RepairMechanicalPawn"), mech)
                {
                    loadID = Find.UniqueIDsManager.GetNextJobID(),
                    hediffs = mech.Damage().ToList()
                };
                parent.Map.physicalInteractionReservationManager.Reserve(drone, job, mech);
                drone.jobs.StartJob(job);
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
            }
        }
    }

    public IEnumerable<MechanicalPawn> MechsAvailableForRepair()
    {
        for (var i = 0; i < radialCells; i++)
        {
            var pos = parent.Position + GenRadial.RadialPattern[i];
            if (!pos.InBounds(parent.Map)) continue;
            var pawn = pos.GetFirstPawn(parent.Map);
            if (pawn == null || !(pawn is MechanicalPawn mech) || !mech.IsDamaged()) continue;
            yield return (MechanicalPawn)pawn;
        }
    }

    public override void PostDraw()
    {
        base.PostDraw();
        if (Find.Selector.IsSelected(parent)) GenDraw.DrawRadiusRing(parent.TrueCenter().ToIntVec3(), Props.radius);
    }

    public override void PostPrintOnto(SectionLayer layer)
    {
        base.PostPrintOnto(layer);
        PrintIdleDrones(layer);
    }

    public void StoreDrone(RepairDrone drone)
    {
        drone.DeSpawn();
        DroneContainer.TryAdd(drone);
        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
    }

    private void PrintIdleDrones(SectionLayer layer)
    {
        for (var i = 0; i < DroneContainer.Count; i++)
        {
            var drawPos = parent.DrawPos + new Vector3(0, AltitudeLayer.BuildingOnTop.AltitudeFor(), 0) +
                          Props.dronePositions[i];
            var drone = DroneContainer[i] as RepairDrone;
            Graphic droneGraphic = drone.Drawer.renderer.graphics.nakedGraphic;
            var mat = droneGraphic.MatNorth;
            Printer_Plane.PrintPlane(layer, drawPos, new Vector2(Props.droneSize, Props.droneSize), mat);
        }
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        //sb.Append(base.CompInspectStringExtra().TrimEndNewlines());
        return sb.ToString().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_Action
        {
            defaultLabel = "Add Drone",
            action = delegate
            {
                AddMech(MakeMech(Props.droneDef));
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
            }
        };
    }
}

public class CompProperties_RepairDrone : CompProperties_MechStation
{
    public MechanicalPawnKindDef droneDef;

    public List<Vector3> dronePositions = new() { new Vector3(0, 0, 0.25f) };
    public float droneSize = 1;
    public float radius = 1;

    public CompProperties_RepairDrone()
    {
        compClass = typeof(Comp_RepairDrone);
    }
}