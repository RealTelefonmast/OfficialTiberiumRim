using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class RoomRoleWorker_AirLock : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            int airlockDoor = 0;
            int outsideConns = 0;
            int vents = 0;
            var things = room.ContainedAndAdjacentThings;
            foreach (var thing in things)
            {
                if (thing is Building_AirLock airLock)
                {
                    //Airlocks only valid when one door connects to outside
                    if (airLock.ConnectsToOutside)
                    {
                        outsideConns++;
                    }
                    airlockDoor++;
                }
            }

            if (outsideConns <= 0)
            {
                return 0f;
            }

            if (airlockDoor >= 2)
            {
                return float.MaxValue;
            }
            return 0f;
        }
    }
}
