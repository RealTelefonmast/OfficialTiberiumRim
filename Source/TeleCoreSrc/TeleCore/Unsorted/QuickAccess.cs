using System.Collections.Generic;
using Verse;

namespace TeleCore.Unsorted;

//TODO: Is this needed?
public static class QuickAccess
{
    private static Dictionary<Thing, RoomTracker> _roomTrackerByThing;

    static QuickAccess()
    {
        _roomTrackerByThing = new();

        GlobalEventHandler.Things.Spawned += HandleThingSpawn;
        GlobalEventHandler.Things.Despawned += HandleThingDespawn;

        GlobalRoomEventHandler.Rooms.Created += HandleCreated;
    }

    private static void HandleRoomCreated(RoomChangedArgs args)
    {
        args.RoomTracker.Room.Regions[0].ListerThings.AllThings.ForEach(thing =>
        {
            if (_roomTrackerByThing.ContainsKey(thing))
            {
                _roomTrackerByThing[thing] = args.RoomTracker;
            }
            else
            {
                _roomTrackerByThing.Add(thing, args.RoomTracker);
            }
        });
    }

    private static void HandleThingDespawn(ThingStateChangedEventArgs args)
    {

    }

    private static void HandleThingSpawn(ThingStateChangedEventArgs args)
    {

    }
}