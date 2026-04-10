using System.Collections.Generic;
using TeleCore.Things;
using TeleCore.Utils;
using Verse;

namespace TeleCore.Atmosphere.Rooms.AirLocks;

public class RoomRoleWorker_AirLock : RoomRoleWorker
{
    public override float GetScore(Room room)
    {
        if (room.UsesOutdoorTemperature) return -1;

        var airlockDoorConns = 0;
        HashSet<Room> knownRooms = new();
        var things = room.ContainedAndAdjacentThings;
        foreach (var thing in things)
            if (thing is Building_Airlock airLock)
                if (knownRooms.Add(airLock.OppositeRoom(room)))
                    airlockDoorConns++;

        knownRooms = null;

        if (airlockDoorConns >= 2) return float.MaxValue;
        return -1;
    }

    public override string PostProcessedLabel(string baseLabel, Room room)
    {
        //var hoveredRoom = UI.MouseCell().GetRoom(Find.CurrentMap);
        var curAirLock = room.GetRoomComp<RoomComponent_AirLock>(); //room?.GetRoomComp<RoomComponent_AirLock>();
        if (curAirLock == null) return base.PostProcessedLabel(baseLabel, room);

        return
            $"{base.PostProcessedLabel(baseLabel, room)} [{(curAirLock.IsActiveAirLock ? "Active" : "Inactive")}][{curAirLock.Room.ID}]";
    }
}