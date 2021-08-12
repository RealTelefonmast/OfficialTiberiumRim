using Verse;
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
