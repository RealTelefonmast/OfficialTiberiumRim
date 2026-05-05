using System;
using TeleCore.Types.Structs;
using Verse;

namespace TeleCore.Types;

internal class DelayedRoomUpdate : IDisposable
{
    public DelayedRoomUpdate(RoomChangeType type, Room room)
    {
        Type = type;
        Room = room;
    }

    public DelayedRoomUpdate(RoomChangeType type, RoomTracker tracker)
    {
        Type = type;
        Tracker = tracker;
        Room = tracker.Room;
    }

    public RoomChangeType Type { get; }
    public RoomTracker Tracker { get; private set; }
    public RoomTracker?[] Previous { get; set; }
    public Room Room { get; private set; }

    public void Dispose()
    {
        Tracker = null;
        Previous = null;
        Room = null;
    }

    public override string ToString()
    {
        return $"{Tracker.Room.ID}:{Type}";
    }

    public void SetTracker(RoomTracker newTracker)
    {
        Tracker = newTracker;
    }

    public void SetPrevious(RoomTracker?[] previous)
    {
        Previous = previous;
    }
}