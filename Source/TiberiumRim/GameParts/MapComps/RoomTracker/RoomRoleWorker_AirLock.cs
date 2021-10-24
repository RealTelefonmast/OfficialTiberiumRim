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
                //int outsideConns = 0;
            //int vents = 0;
            var things = room.ContainedAndAdjacentThings;
            foreach (var thing in things)
            {
                if (thing is Building_AirLock airLock)
                {
                    //Airlocks only valid when one door connects to outside
                    //if (airLock.ConnectsToOutside)
                    //{
                    //    outsideConns++;
                    //}
                    if(knownRooms.Add(airLock.OppositeRoom(room)))
                        airlockDoorConns++;
                }
            }

            //if (outsideConns <= 0)
            //{
            //    return 0f;
            //}

            if (airlockDoorConns >= 2)
            {
                return float.MaxValue;
            }
            return 0f;
        }

        public override string PostProcessedLabel(string baseLabel)
        {
            var selectedThing = Find.Selector.FirstSelectedObject as Thing;
            //var curRoom = selectedThing != null ? selectedThing.GetRoom() : UI.MouseCell().GetRoom(Find.CurrentMap);
            var curAirLock = UI.MouseCell().GetRoom(Find.CurrentMap).GetRoomComp<RoomComponent_AirLock>();

            if (curAirLock == null) return base.PostProcessedLabel(baseLabel);

            return $"{base.PostProcessedLabel(baseLabel)} [{(curAirLock.IsActive ? "Active" : "Inactive")}][{curAirLock.PawnQueue.Count}][{curAirLock.CurrentPawn}]";
        }
    }
}
