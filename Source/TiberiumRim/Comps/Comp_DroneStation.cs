using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Comp_DroneStation : Comp_MechStation
    {
        private int radialCells;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            radialCells = GenRadial.NumCellsInRadius(Props.radius);
            if (MainMechLink.LinkedMechs.NullOrEmpty())
            {
                for (int i = 0; i < Props.garageCapacity; i++)
                {
                    TryAddMech(MakeMech(Props.mechKindDef), true);
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public new CompProperties_DroneStation Props => base.props as CompProperties_DroneStation;
        public ThingOwner DroneGarage => MainGarage.Container;
        public CompRefuelable FuelComp => parent.GetComp<CompRefuelable>();

        private PhysicalInteractionReservationManager Reservations => parent.Map.physicalInteractionReservationManager;

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
            if (!FuelComp.HasFuel) return;
            var mechs = MechsAvailableForRepair().ToList();
            if (!mechs.Any()) return;
            for (var i = MainMechLink.Count - 1; i >= 0; i--)
            {
                var drone = (RepairDrone)MainMechLink[i];
                foreach (var damagedMech in MechsAvailableForRepair())
                {
                    if (Reservations.IsReserved(damagedMech)) continue;
                    var closestPos = GenAdjFast.AdjacentCells8Way(parent).MinBy(c => c.DistanceTo(damagedMech.Position));
                    if (drone.Spawned && Reservations.FirstReservationFor(drone) != null) continue;
                    if (!drone.Spawned && !MainGarage.TryPullFromGarage(drone, out Thing mech, closestPos, parent.Map, ThingPlaceMode.Direct)) continue;

                    var job = new JobWithExtras(DefDatabase<JobDef>.GetNamed("RepairMechanicalPawn"), damagedMech)
                    {
                        loadID = Find.UniqueIDsManager.GetNextJobID(),
                        hediffs = damagedMech.Damage().ToList()
                    };
                    Reservations.Reserve(drone, job, damagedMech);
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
            MainGarage.TryPushToGarage(drone);
            parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
        }

        private void PrintIdleDrones(SectionLayer layer)
        {
            for(int i = 0; i < DroneGarage.Count; i++)
            {
                Vector3 drawPos = parent.DrawPos + new Vector3(0, AltitudeLayer.BuildingOnTop.AltitudeFor(), 0) + Props.renderOffsets[i];
                RepairDrone drone = DroneGarage[i] as RepairDrone;
                Graphic droneGraphic = drone.Drawer.renderer.graphics.nakedGraphic;
                Material mat = droneGraphic.MatSouth;
                Printer_Plane.PrintPlane(layer, drawPos, new Vector2(Props.renderSize, Props.renderSize), mat, 0, false);
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
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
                    parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
                }
            };
        }
    }

    public class CompProperties_DroneStation : CompProperties_MechStation
    {
        public float radius = 1;

        //Render Props
        public float renderSize = 1f;
        public List<Vector3> renderOffsets = new List<Vector3>() {new Vector3(0, 0, 0.25f)};

        public CompProperties_DroneStation()
        {
            this.compClass = typeof(Comp_DroneStation);
        }
    }
}