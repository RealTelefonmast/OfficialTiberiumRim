using System.Text.RegularExpressions;

namespace TeleCore.Unsorted;

public class RoomComponent_AirLock : RoomComponent
{
    public bool IsActive => Group.Rooms.All(r => r.Role == TiberiumDefOf.TR_AirLock);
}