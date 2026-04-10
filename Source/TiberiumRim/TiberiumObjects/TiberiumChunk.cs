using RimWorld;
using TeleCore.Utils;
using Verse;

namespace TR;

public class TiberiumChunk : TRThing, IPathFindCostProvider
{
    private const ushort Cost_ChunkAvoid = 400;

    public ushort PathFindCostFor(Pawn pawn)
    {
        if (!pawn.CanBeAffectedByTib())
            return 0;
        return Cost_ChunkAvoid;
    }

    public CellRect GetOccupiedRect()
    {
        return this.OccupiedRect();
    }

    public override void TickRare()
    {
        base.TickRare();
        if (TRandom.Chance(0.1f))
            TakeDamage(new DamageInfo(DamageDefOf.Deterioration, TRandom.Range(0, 3)));
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        base.Destroy(mode);
    }
}