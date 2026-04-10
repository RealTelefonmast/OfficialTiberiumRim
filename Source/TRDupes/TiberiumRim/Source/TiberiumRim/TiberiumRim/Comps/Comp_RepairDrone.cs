using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class Comp_RepairDrone : Comp_MechStation
    {
        public int radialCells;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            radialCells = GenRadial.NumCellsInRadius(Props.radius);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public new CompProperties_RepairDrone Props => base.props as CompProperties_RepairDrone;

        public ThingOwner DroneContainer => base.GetDirectlyHeldThings();

        public override void CompTick()
        {
            base.CompTick();
            if (IsPowered)
            {
                TryReleaseDrone();
            }
        }

        public void TryReleaseDrone()
        {
            var mechs = MechsAvailableForRepair().ToList();
            if (!mechs.Any()) return;
            foreach (var drone in storedMechs)
            {
                if (!DroneContainer.Contains(drone)) continue;
                foreach (var mech in mechs)
                {
                    if (parent.Map.physicalInteractionReservationManager.IsReserved(mech)) continue;
                    var closestPos = GenAdjFast.AdjacentCells8Way(parent).MinBy(c => c.DistanceTo(mech.Position));
                    DroneContainer.TryDrop(drone, closestPos, parent.Map, ThingPlaceMode.Direct, out Thing last);
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
            for (int i = 0; i < radialCells; i++)
            {
                var pos = this.parent.Position + GenRadial.RadialPattern[i];
                if (!pos.InBounds(parent.Map)) continue;
                Pawn pawn = pos.GetFirstPawn(parent.Map);
                if (pawn == null || !(pawn is MechanicalPawn mech) || !mech.IsDamaged()) continue;
                yield return (MechanicalPawn)pawn;
            }
        }

        public bool IsPowered
        {
            get
            {
                CompPowerTrader comp = this.parent.TryGetComp<CompPowerTrader>();
                return comp?.PowerOn ?? true;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (Find.Selector.IsSelected(this.parent))
            {
                GenDraw.DrawRadiusRing(this.parent.TrueCenter().ToIntVec3(), Props.radius);
            }
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
            for(int i = 0; i < DroneContainer.Count; i++)
            {
                Vector3 drawPos = parent.DrawPos + new Vector3(0, AltitudeLayer.BuildingOnTop.AltitudeFor(), 0) + Props.dronePositions[i];
                RepairDrone drone = DroneContainer[i] as RepairDrone;
                Graphic droneGraphic = drone.Drawer.renderer.graphics.nakedGraphic;
                Material mat = droneGraphic.MatNorth;
                Printer_Plane.PrintPlane(layer, drawPos, new Vector2(Props.droneSize, Props.droneSize), mat, 0, false);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "Add Drone",
                action = delegate
                {
                    MakeMech(Props.droneDef);
                    parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
                }
                
            };
        }
    }

    public class CompProperties_RepairDrone : CompProperties_MechStation
    {
        public float radius = 1;
        public float droneSize = 1;
        public MechanicalPawnKindDef droneDef;

        public List<Vector3> dronePositions = new List<Vector3>() {new Vector3(0, 0, 0.25f)};

        public CompProperties_RepairDrone()
        {
            this.compClass = typeof(Comp_RepairDrone);
        }
    }
}