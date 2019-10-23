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
    public class Comp_RepairDrone : Comp_Upgradable, IThingHolder
    {
        public List<RepairDrone> repairDrones = new List<RepairDrone>();
        public int radialCells;

        private ThingOwner droneContainer;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            radialCells = GenRadial.NumCellsInRadius(Props.radius);
            droneContainer = new ThingOwner<Thing>(this, false);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref repairDrones, "repairDrones", LookMode.Reference);
        }

        public CompProperties_RepairDrone Props => base.props as CompProperties_RepairDrone;

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
            Log.Message("Mechs: " + mechs.Count, true);
            for (int i = 0; i < repairDrones.Count; i++)
            {
                var drone = repairDrones[i];
                if (!droneContainer.Contains(drone)) continue;
                foreach (var mech in mechs)
                {
                    if(parent.Map.physicalInteractionReservationManager.IsReserved(mech)) continue;
                    var closestPos = GenAdjFast.AdjacentCells8Way(parent).MinBy(c => c.DistanceTo(mech.Position));
                    droneContainer.TryDrop(drone, closestPos, parent.Map, ThingPlaceMode.Direct, out Thing last);
                    var job = new JobWithExtras(DefDatabase<JobDef>.GetNamed("RepairMechanicalPawn"), mech);
                    parent.Map.physicalInteractionReservationManager.Reserve(drone, job, mech);
                    job.hediffs = mech.Damage().ToList();
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

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            RemoveDrones();
            base.PostDestroy(mode, previousMap);
        }

        public void RemoveDrones()
        {
            repairDrones.ForEach(d => d.DeSpawn());
            //TODO: Add Job
            return;
            repairDrones.ForEach(d =>
            {
                d.jobs.ClearQueuedJobs();
                d.jobs.EndCurrentJob(JobCondition.InterruptForced);
                d.jobs.StartJob(new Job(DefDatabase<JobDef>.GetNamed("RepairMechanicalPawn"), parent.Position));
            });
        }

        public RepairDrone MakeDrone()
        {
            RepairDrone drone = (RepairDrone)PawnGenerator.GeneratePawn(Props.droneDef, parent.Faction);
            drone.ageTracker.AgeBiologicalTicks = 0;
            drone.ageTracker.AgeChronologicalTicks = 0;
            drone.Rotation = Rot4.Random;
            drone.parent = this.parent as Building;
            drone.Drawer.renderer.graphics.ResolveAllGraphics();

            repairDrones.Add(drone);
            droneContainer.TryAdd(drone);
            return drone;
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
            droneContainer.TryAdd(drone);
            parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
        }

        private void PrintIdleDrones(SectionLayer layer)
        {
            for(int i = 0; i < droneContainer.Count; i++)
            {
                Vector3 drawPos = parent.DrawPos + new Vector3(0, AltitudeLayer.BuildingOnTop.AltitudeFor(), 0) + Props.dronePositions[i];
                RepairDrone drone = droneContainer[i] as RepairDrone;
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
                    MakeDrone();
                    parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
                }
                
            };
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return droneContainer;
        }
    }

    public class CompProperties_RepairDrone : CompProperties_Upgrade
    {
        public int droneAmount = 1;
        public float radius = 1;
        public float droneSize = 1;
        public PawnKindDef droneDef;

        public List<Vector3> dronePositions = new List<Vector3>() {new Vector3(0, 0, 0.25f)};

        public CompProperties_RepairDrone()
        {
            this.compClass = typeof(Comp_RepairDrone);
        }
    }
}