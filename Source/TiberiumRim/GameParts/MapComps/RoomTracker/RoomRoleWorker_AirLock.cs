using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class RoomRoleWorker_AirLock : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            int airlockDoorConns = 0;
            HashSet<Room> knownRooms = new();
            var things = room.ContainedAndAdjacentThings;
            foreach (var thing in things)
            {
                if (thing is Building_AirLock airLock)
                {
                    if (knownRooms.Add(airLock.OppositeRoom(room)))
                        airlockDoorConns++;
                }
            }
            knownRooms = null;

            if (airlockDoorConns >= 2)
            {
                return float.MaxValue;
            }
            return 0f;
        }

        public override string PostProcessedLabel(string baseLabel)
        {
            var room = Find.Selector.SingleSelectedThing?.GetRoom() ?? UI.MouseCell().GetRoom(Find.CurrentMap);
            var curAirLock = room?.GetRoomComp<RoomComponent_AirLock>();
            if (curAirLock == null) return base.PostProcessedLabel(baseLabel);

            return $"{base.PostProcessedLabel(baseLabel)} [{(curAirLock.IsActiveAirLock ? "Active" : "Inactive")}][{curAirLock.Room.ID}]";
        }
    }
}
