using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class RepairDroneKindDef : MechanicalPawnKindDef
    {
        public float healFloat = 0.01f;
    }

    public class RepairDrone : MechanicalPawn, IPawnWithParent
    {
        public new RepairDroneKindDef kindDef => base.kindDef as RepairDroneKindDef;
        public Comp_DroneStation parentComp;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            this.parentComp = this.parent?.GetComp<Comp_DroneStation>();
            if(parent == null)
                Log.Warning("RepairDrone Spawned without parent");
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            this.DeSpawn();
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
    }
}
