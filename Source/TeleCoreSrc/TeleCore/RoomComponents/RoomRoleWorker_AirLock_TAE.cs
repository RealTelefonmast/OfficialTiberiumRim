// Preserved from TeleCore/Airlock/RoomRoleWorker_AirLock.cs

using System.Collections.Generic;
using TeleCore.Buildings;
using Verse;

namespace TeleCore.RoomComponents;

public class RoomRoleWorker_AirLock_TAE : RoomRoleWorker
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

    public override string PostProcessedLabel(string baseLabel)
    {
        var hoveredRoom = UI.MouseCell().GetRoom(Find.CurrentMap);
        var selRoom = Find.Selector.SingleSelectedThing?.GetRoom();
        var curAirLock = (hoveredRoom ?? selRoom).GetRoomComp<TAE.RoomComponent_AirLock>();
        if (curAirLock == null) return base.PostProcessedLabel(baseLabel);

        return
            $"{base.PostProcessedLabel(baseLabel)} [{(curAirLock.IsActiveAirLock ? "Active" : "Inactive")}][{curAirLock.Room.ID}]";
    }
}