using TeleCore.RoomComponents;

namespace TeleCore.Comps;

public class Comp_ANS_AirLockVent : Comp_ANS_AirVent
{
    private RoomComponent_AirLock airlockComp;

    public void SetAirLock(RoomComponent_AirLock roomComponentAirLock)
    {
        airlockComp = roomComponentAirLock;
    }
}