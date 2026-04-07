using RimWorld;
using Verse;

namespace TR.DefOf;

[RimWorld.DefOf]
public static class TRPawnRenderNodeTagDefOf
{
    public static PawnRenderNodeTagDef TibHead;
    public static PawnRenderNodeTagDef TibBody;

    static TRPawnRenderNodeTagDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(TRPawnRenderNodeTagDefOf));
    }
}