using Verse;

namespace TeleCore.Unsorted;

internal struct DelayedCacheAction
{
    public RegionStateChangedArgs Args { get; }
    public IntVec3 Cell { get; }
    public int Index { get; }

    public DelayedCacheAction(RegionStateChangedArgs args, IntVec3 cell, int index)
    {
        Args = args;
        Cell = cell;
        Index = index;
    }
}