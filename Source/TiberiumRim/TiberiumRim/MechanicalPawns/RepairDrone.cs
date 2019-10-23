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
        public new RepairDroneKindDef KindKindDef;
        public Comp_RepairDrone parentComp;
        public Building parent;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            this.KindKindDef = base.kindDef as RepairDroneKindDef;
            this.parentComp = this.parent.GetComp<Comp_RepairDrone>();
            base.SpawnSetup(map, respawningAfterLoad);
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
