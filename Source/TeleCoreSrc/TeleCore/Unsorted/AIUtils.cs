using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TeleCore.Unsorted;

public static class AIUtils
{
    public static void RoomsAlongPath(this List<IntVec3> pathNodes, ref List<Room> roomList, Verse.Map map,
        bool ignoreDoorWays = true, bool reverse = false, RoomRoleDef withRole = null)
    {
        roomList.Clear();
        Room lastAddedRoom = null;
        for (var i = 0; i < pathNodes.Count; i++)
        {
            var node = pathNodes[reverse ? pathNodes.Count - 1 - i : i];
            var newRoom = node.GetRoom(map);
            if (ignoreDoorWays && newRoom.IsDoorway) continue;
            if (newRoom == lastAddedRoom) continue;
            if (withRole != null && newRoom.Role != withRole) continue;
            roomList.Add(newRoom);
            lastAddedRoom = newRoom;
        }
    }

    public static void RoomsAlongPath(this List<IntVec3> pathNodes, ref List<RoomTracker> roomList, Verse.Map map,
        bool ignoreDoorWays = true, bool reverse = false, RoomRoleDef withRole = null)
    {
        roomList.Clear();
        Room lastAddedRoom = null;
        for (var i = 0; i < pathNodes.Count; i++)
        {
            var node = pathNodes[reverse ? pathNodes.Count - 1 - i : i];
            var newRoom = node.GetRoom(map);
            if (ignoreDoorWays && newRoom.IsDoorway) continue;
            if (newRoom == lastAddedRoom) continue;
            if (withRole != null && newRoom.Role != withRole) continue;
            roomList.Add(newRoom.RoomTracker());
            lastAddedRoom = newRoom;
        }
    }

    public static PawnPathResult TryGetPath(IntVec3 start, LocalTargetInfo destination, Verse.Map map,
        TraverseParms traverseParms, PathEndMode endMode = PathEndMode.OnCell)
    {
        if (map.reachability.CanReach(start, destination, endMode, traverseParms))
            return new PawnPathResult(map.pathFinder.FindPath(start, destination, traverseParms, endMode));
        return new PawnPathResult(null, "Cannot reach destination.");
    }

    /*TODO: probably part of the airlocks?
    public static IntVec3 GeneralCenter(this Room room)
    {
        var poll = room.AtmosphericRoomComp();
        var size = poll.Parent.Size / 2;
        return room.Cells.First(t =>
        {
            var vec = (t - poll.Parent.MinVec);
            return (vec.x >= size.x && vec.z >= size.z) || (vec.x <= size.x && vec.z <= size.z);
        });
        //return poll.Parent.MinVec + new IntVec3(size.x/2,0, size.z/2);
    }
    */
}