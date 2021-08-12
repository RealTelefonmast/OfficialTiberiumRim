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
        public static List<Room> RoomsAlongPath(this PawnPath path, Map map)
        {
            HashSet<Room> rooms = new HashSet<Room>();
            foreach (var node in path.NodesReversed)
            {
                rooms.Add(node.GetRoom(map));
            }
            return rooms.ToList();
        }

        public static IntVec3 GeneralCenter(this Room room)
        {
            var poll = room.Pollution();
            var size = poll.Size;
            return poll.MinVec + new IntVec3(size.x/2,0, size.z/2);
        }
    }
}
