using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TiberiumRim
{
    public class RepairDroneDef : PawnKindDef
    {
        public float healFloat = 0.1f;
    }

    public class RepairDrone : Mechanical_Pawn
    {
        public new RepairDroneDef kindDef;

        public Comp_RepairDrone parentComp;

        public Building parent;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            this.kindDef = base.kindDef as RepairDroneDef;
            this.parentComp = this.parent.GetComp<Comp_RepairDrone>();
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public int RadialCells
        {
            get
            {
                return GenRadial.NumCellsInRadius(this.parentComp.repairProps.radius);
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref parent, "parent");
            base.ExposeData();
        }

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            this.DeSpawn();
        }

        public Mechanical_Pawn AvailableMech
        {
            get
            {
                for (int i = 0; i < RadialCells; i++)
                {
                    IntVec3 pos = this.parent.Position + GenRadial.RadialPattern[i];
                    if (pos.InBounds(this.Map))
                    {
                        Pawn pawn = pos.GetFirstPawn(this.Map);
                        if (pawn != null && pawn is Mechanical_Pawn && !(pawn is RepairDrone) && pawn.health.hediffSet.hediffs.Find((Hediff x) => x.Severity > 0) != null)
                        {
                            return pawn as Mechanical_Pawn;
                        }
                    }
                }
                return null;
            }
        }
    }
}
