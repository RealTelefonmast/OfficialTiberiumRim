using TeleCore.Events;
using TeleCore.Events.Args;
using Verse;

namespace TeleCore.Systems.RoomTracking.Updates;

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