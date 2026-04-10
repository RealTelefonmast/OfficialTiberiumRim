using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Comp_RepairDrone : Comp_Upgrade
    {
        public List<RepairDrone> repairDrones = new List<RepairDrone>();

        public CompProperties_RepairDrone repairProps;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.repairProps = (CompProperties_RepairDrone)this.props;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref repairDrones, "repairDrones", LookMode.Reference);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (IsPowered)
            {
                if (repairDrones.Count < repairProps.droneAmount && Find.TickManager.TicksGame % 750 == 0)
                {
                    RepairDrone drone = SpawnDrone();
                    repairDrones.Add(drone);
                }
            }
            else{RemoveDrones();}
        }

        public bool IsPowered
        {
            get
            {   CompPowerTrader comp = this.parent.TryGetComp<CompPowerTrader>();
                if(comp == null)
                {
                    return true;
                }
                if (comp.PowerOn)
                {
                    return true;
                }
                return false;
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            RemoveDrones();
            base.PostDestroy(mode, previousMap);
        }

        public void RemoveDrones()
        {
            foreach(RepairDrone drone in repairDrones)
            {
                if (drone != null)
                {
                    drone.Destroy();
                }              
            }
            repairDrones.Clear();
        }

        public RepairDrone SpawnDrone()
        {
            RepairDrone drone = (RepairDrone)PawnGenerator.GeneratePawn(repairProps.droneDef, parent.Faction);
            drone.ageTracker.AgeBiologicalTicks = 0;
            drone.ageTracker.AgeChronologicalTicks = 0;
            drone.Rotation = Rot4.Random;
            drone.parent = this.parent as Building;           
            IntVec3 spawnLoc = this.parent.Position;
            return (RepairDrone)GenSpawn.Spawn(drone, spawnLoc, this.parent.Map);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (Find.Selector.IsSelected(this.parent))
            {
                GenDraw.DrawRadiusRing(this.parent.TrueCenter().ToIntVec3(), repairProps.radius);
            }
        }
    }

    public class CompProperties_RepairDrone : CompProperties_Upgrade
    {
        public int droneAmount = 1;

        public float radius = 1;

        public PawnKindDef droneDef;

        public CompProperties_RepairDrone()
        {
            this.compClass = typeof(Comp_RepairDrone);
        }
    }
}
