using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace TiberiumRim.Utilities
{
    public static class AIUtils
    {
        public static List<Room> RoomsAlongPath(this PawnPath path, Map map, RoomRoleDef withRole = null)
        {
            List<Room> rooms = new();
            Room lastAddedRoom = null;
            for (var i = 0; i < path.NodesReversed.Count; i++)
            {
                var node = path.NodesReversed[i];
                var newRoom = node.GetRoom(map);
                if(newRoom == lastAddedRoom) continue;
                if(withRole != null && newRoom.Role != withRole) continue;
                rooms.Add(newRoom);
                lastAddedRoom = newRoom;
            }
            return rooms;
        }

        public static IntVec3 GeneralCenter(this Room room)
        {
            var poll = room.AtmosphericRoomComp();
            var size = poll.Parent.Size/2;
            return room.Cells.First(t =>
            {
                var vec = (t - poll.Parent.MinVec);
                return (vec.x >= size.x && vec.z >= size.z) || (vec.x <= size.x && vec.z <= size.z);
            });
            //return poll.Parent.MinVec + new IntVec3(size.x/2,0, size.z/2);
        }
    }
}
