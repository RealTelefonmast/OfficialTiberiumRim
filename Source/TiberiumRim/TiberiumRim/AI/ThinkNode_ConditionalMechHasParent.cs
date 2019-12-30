using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class ThinkNode_ConditionalMechHasParent : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return (pawn as MechanicalPawn)?.ParentBuilding != null;
        }
    }
}
