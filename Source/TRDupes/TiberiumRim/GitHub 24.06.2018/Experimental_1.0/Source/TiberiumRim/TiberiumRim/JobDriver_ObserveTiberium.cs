using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobDriver_ObserveTiberium : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            WorldComponent_TiberiumMissions research = Find.World.GetComponent<WorldComponent_TiberiumMissions>();

            if (pawn.Downed || pawn.CarriedBy != null || pawn.Drafted || pawn.InAggroMentalState || pawn.InContainerEnclosed)
            {
                return null;
            }
            IntVec3 spot = pawn.Map.listerThings.AllThings.Find((Thing x) => x is Building_TiberiumProducer || x is TiberiumCrystal).Position;
            if (spot.IsValid)
            {
                int radNum = GenRadial.NumCellsInRadius(8);
                for (int i = 0; i < radNum; i++)
                {
                    IntVec3 v = spot + GenRadial.RadialPattern[i];
                    if (v.Standable(pawn.Map) && pawn.CanReach(v, PathEndMode.OnCell, Danger.None))
                    {
                        JobDef job = DefDatabase<JobDef>.GetNamed("ObserveTiberium");
                        return new Job(job, v);
                    }
                }
            }    
            return null;
        }
    }

}
