using TR.Hediffs;
using Verse;

namespace TR.VeinholeSystem;

public class VeinGasCloud : HomingThing, IPathFindCostProvider
{
    private const ushort Cost_GasAvoid = 1000;

    public ushort PathFindCostFor(Pawn pawn)
    {
        if (!pawn.CanBeAffectedByTib(true))
            return 0;
        return Cost_GasAvoid;
    }

    public CellRect GetOccupiedRect()
    {
        return this.OccupiedRect();
    }

    public override void Tick()
    {
        base.Tick();
        foreach (var intVec3 in Position.CellsAdjacent8Way())
        {
            var pawn = intVec3.GetFirstPawn(Map);
            if (pawn != null)
                HediffUtils.TryInfectPawn(pawn, 1, true, 1);
        }
    }
}