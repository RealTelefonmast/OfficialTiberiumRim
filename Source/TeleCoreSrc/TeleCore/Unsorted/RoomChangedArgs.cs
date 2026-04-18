namespace TeleCore.Unsorted;

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