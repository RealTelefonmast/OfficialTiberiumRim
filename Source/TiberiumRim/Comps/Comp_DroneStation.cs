using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using TR.DefOf;
using TR.ThingData.Jobs;
using TR.ThingData.Pawns.MechanicalPawns;
using UnityEngine;
using Verse;

namespace TR.Comps;

public class Comp_DroneStation : Comp_MechStation
{
    private int radialCells;

    public new CompProperties_DroneStation Props => props as CompProperties_DroneStation;
    public ThingOwner DroneGarage => MainGarage.Container;
    public CompRefuelable FuelComp => parent.GetComp<CompRefuelable>();

    private PhysicalInteractionReservationManager Reservations => parent.Map.physicalInteractionReservationManager;

    public bool IsPowered
    {
        get
        {
            var comp = parent.TryGetComp<CompPowerTrader>();
            return comp?.PowerOn ?? true;
        }
    }

    public new CompProperties_DroneStation Props => props as CompProperties_DroneStation;
    public ThingOwner DroneGarage => MainGarage.Container;
    public CompRefuelable FuelComp => parent.GetComp<CompRefuelable>();

    private PhysicalInteractionReservationManager Reservations => parent.Map.physicalInteractionReservationManager;

    //TODO: A property getter with a for loop? This should be a state..
    public bool AnyMechAvailableForRepair
    {
        get
        {
            for (var i = 0; i < radialCells; i++)
            {
                var pos = parent.Position + GenRadial.RadialPattern[i];
                if (!pos.InBounds(parent.Map)) continue;
                var pawn = pos.GetFirstPawn(parent.Map);
                if (pawn is not MechanicalPawn mech || !mech.IsDamaged()) continue;
                return true;
            }

            return false;
        }
    }

    public IEnumerable<MechanicalPawn> MechsAvailableForRepair
    {
        get
        {
            for (var i = 0; i < radialCells; i++)
            {
                var pos = parent.Position + GenRadial.RadialPattern[i];
                if (!pos.InBounds(parent.Map)) continue;
                var pawn = pos.GetFirstPawn(parent.Map);
                if (pawn is not MechanicalPawn mech || !mech.IsDamaged()) continue;
                yield return (MechanicalPawn)pawn;
            }
        }
    }

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
        if (MainMechLink.LinkedMechs.NullOrEmpty())
            for (var i = 0; i < Props.garageCapacity; i++)
                TryAddMech(MakeMech(Props.mechKindDef), true);
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
        if (!FuelComp.HasFuel) return;
        if (MainMechLink.Count == 0) return;
        if (!AnyMechAvailableForRepair) return;
        foreach (var brokeyMech in MechsAvailableForRepair)
            for (var i = MainMechLink.Count - 1; i >= 0; i--)
            {
                var drone = (RepairDrone)MainMechLink[i];
                if (Reservations.IsReserved(brokeyMech)) continue;

                var closestPos = GenAdjFast.AdjacentCells8Way(parent).MinBy(c => c.DistanceTo(brokeyMech.Position));
                if (drone.Spawned && Reservations.FirstReservationFor(drone) != null) continue;
                if (!drone.Spawned &&
                    !MainGarage.TryPullFromGarage(drone, out var mech, closestPos, parent.Map)) continue;
                var job = new JobWithExtras(TRCDefOf.RepairMechanicalPawn, brokeyMech)
                {
                    loadID = Find.UniqueIDsManager.GetNextJobID(),
                    hediffs = brokeyMech.Damage().ToList()
                };
                Reservations.Reserve(drone, job, brokeyMech);
                drone.jobs.StartJob(job);
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
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
        MainGarage.TryPushToGarage(drone);
        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
    }

    private void PrintIdleDrones(SectionLayer layer)
    {
        if (DroneGarage.Count == 0) return;
        for (var i = 0; i < DroneGarage.Count; i++)
        {
            var drawPos = parent.DrawPos + new Vector3(0, AltitudeLayer.BuildingOnTop.AltitudeFor(), 0) +
                          Props.renderOffsets[i];
            var drone = (DroneGarage as ThingOwner<MechanicalPawn>).innerList[0] as RepairDrone;
            var droneGraphic = drone.Drawer.renderer.BodyGraphic;
            var mat = droneGraphic.MatSouth;
            Printer_Plane.PrintPlane(layer, drawPos, new Vector2(Props.renderSize, Props.renderSize), mat);
        }
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        sb.AppendLine("TR_RepairDrones".Translate(DroneGarage.Count));
        sb.AppendLine("Garage: " + MainGarage.Container.Count);
        sb.AppendLine("Linked: " + MainMechLink.LinkedMechs.Count);
        return sb.ToString().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_Action
        {
            defaultLabel = "Add Drone",
            action = delegate
            {
                TryAddMech(MakeMech(Props.mechKindDef), true);
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
            }
        };
    }
}

public class CompProperties_DroneStation : CompProperties_MechStation
{
    public float radius = 1;
    public List<Vector3> renderOffsets = new() { new Vector3(0, 0, 0.25f) };

    //Render Props
    public float renderSize = 1f;

    public CompProperties_DroneStation()
    {
        compClass = typeof(Comp_DroneStation);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef)) yield return error;
        if (!parentDef.comps.Exists(c => c is CompProperties_Refuelable))
            yield return "A drone station requires a refuelable comp!";
    }
}