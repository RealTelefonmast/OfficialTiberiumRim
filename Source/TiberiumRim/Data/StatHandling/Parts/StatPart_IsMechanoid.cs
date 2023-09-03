using RimWorld;
using Verse;

namespace TR;

public class StatPart_IsMechanoid : StatPart_Tiberium
{
    public override bool IsDisabledFor(Thing thing)
    {
        return thing is Pawn p && !p.IsMechanoid();
    }

    public override float Value(StatRequest req)
    {
        if (req.Thing is Pawn pawn && pawn.IsMechanoid())
            return 1f;
        return 0f;
    }

    public override string Explanation(StatRequest req)
    {
        return $"{"TR_StatPartIsMechanoid".Translate()}: {ValueString(req)}";
    }
}