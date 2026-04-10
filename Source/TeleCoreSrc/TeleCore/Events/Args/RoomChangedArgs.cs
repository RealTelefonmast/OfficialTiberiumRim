using TeleCore.Systems.RoomTracking;

namespace TeleCore.Events.Args;

public enum RoomChangeType
{
    Created,
    Disbanded,
    Reused
}

public struct RoomChangedArgs
{
    public RoomChangeType ChangeType { get; }
    public RoomTracker RoomTracker { get; }

    public RoomChangedArgs(RoomChangeType created, RoomTracker actionTracker)
    {
        ChangeType = created;
        RoomTracker = actionTracker;
    }
}