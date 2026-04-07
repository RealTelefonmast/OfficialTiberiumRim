using System.Collections.Generic;
using TeleCore.Things;

namespace TeleCore.Atmosphere.Rooms.AirLocks;

/// <summary>
/// A singular system for an airlock room.
/// </summary>
public class AirLockSystem
{
    //Data
    private RoomComponent_AirLock _roomComp;
    private List<Building_Airlock> _airLocks;

    //Settings


    public AirLockSystem()
    {
        _airLocks = new List<Building_Airlock>();
    }

    public void RegisterAirLock(Building_Airlock airLock)
    {
        _airLocks.Add(airLock);
    }

    public void DeregisterAirLock(Building_Airlock airLock)
    {
        _airLocks.Remove(airLock);
    }
}
