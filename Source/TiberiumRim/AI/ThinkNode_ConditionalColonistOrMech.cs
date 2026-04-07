using TR.Util;
using Verse;
using Verse.AI;

namespace TR.AI;

public class ThinkNode_ConditionalColonistOrMech : ThinkNode_Conditional
{
    public override bool Satisfied(Pawn pawn)
    {
        return pawn.IsColonist || pawn.IsPlayerControlledMech();
    }
}