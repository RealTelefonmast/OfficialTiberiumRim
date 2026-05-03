using Verse;

namespace TeleCore.Unsorted;

public class RoomRoleWorker_AirLock : RoomRoleWorker
{
    public override float GetScore(Room room)
    {
        var airlockDoor = 0;
        var outsideConns = 0;
        var vents = 0;
        var things = room.ContainedAndAdjacentThings;
        foreach (var thing in things)
            if (thing is Building_AirLock airLock)
            {
                //Airlocks only valid when one door connects to outside
                if (airLock.ConnectsToOutside) outsideConns++;
                airlockDoor++;
            }

        if (outsideConns <= 0) return 0f;

        if (airlockDoor >= 2) return float.MaxValue;
        return 0f;
    }
}