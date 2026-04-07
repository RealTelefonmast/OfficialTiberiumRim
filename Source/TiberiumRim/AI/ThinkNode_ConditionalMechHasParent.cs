using TR.ThingData.Pawns.MechanicalPawns;
using Verse;
using Verse.AI;

namespace TR.AI;

public class ThinkNode_ConditionalMechHasParent : ThinkNode_Conditional
{
    public override bool Satisfied(Pawn pawn)
    {
        return (pawn as MechanicalPawn)?.ParentBuilding != null;
    }
}