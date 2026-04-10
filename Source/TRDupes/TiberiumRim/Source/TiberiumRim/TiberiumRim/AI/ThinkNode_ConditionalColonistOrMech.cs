using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace TiberiumRim
{
    public class ThinkNode_ConditionalColonistOrMech : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.IsColonist || pawn.IsPlayerControlledMech();
        }
    }
}
