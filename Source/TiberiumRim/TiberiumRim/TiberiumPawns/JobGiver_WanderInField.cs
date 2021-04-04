using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobGiver_WanderInField : JobGiver_Wander
    {
        public JobGiver_WanderInField()
        {
            this.wanderRadius = 8f;
            this.locomotionUrgency = LocomotionUrgency.Walk;
            this.ticksBetweenWandersRange = new IntRange(300, 345);
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            return pawn.Position;
        }

        protected override IntVec3 GetExactWanderDest(Pawn pawn)
        {
            if (pawn is TiberiumPawn pawn2 && pawn2.ProducerAvailable && !pawn2.kindDef.canLeaveProducer)
                return pawn2.boundProducer.FieldCells.RandomElement();

            return base.GetExactWanderDest(pawn);
        }
    }
}
