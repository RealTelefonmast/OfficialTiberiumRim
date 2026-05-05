// Preserved from TeleCore/Airlock/AirLockSystem.cs

using System.Collections.Generic;
using TeleCore.Buildings;

namespace TeleCore.Unsorted;

/// <summary>
///     A singular system for an airlock room.
/// </summary>
public class AirLockSystem_TAE
{
    private readonly List<Building_Airlock> _airLocks;

    //Data
    private TAE.RoomComponent_AirLock _roomComp;

    public AirLockSystem_TAE()
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