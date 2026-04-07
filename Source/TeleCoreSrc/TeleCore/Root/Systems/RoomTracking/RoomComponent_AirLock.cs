using System.Text.RegularExpressions;

namespace TeleCore.Systems.RoomTracking;

public class RoomComponent_AirLock : RoomComponent
{
    public bool IsActive => Group.Rooms.All(r => r.Role == TiberiumDefOf.TR_AirLock);
}