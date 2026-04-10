using Verse;
using Verse.AI;

namespace TR;

public class ThinkNode_ConditionalMechHasParent : ThinkNode_Conditional
{
    public override bool Satisfied(Pawn pawn)
    {
        return (pawn as MechanicalPawn)?.ParentBuilding != null;
    }
}