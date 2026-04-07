namespace TeleCore.FlowCore;

public class Comp_Network_Power : CompNetwork
{
    private CompPowerPlant_Network _powerComp;

    public override bool RoleIsActive(NetworkRole role)
    {
        return role switch
        {
            NetworkRole.Requester => !_powerComp.IsAtCapacity,
            _ => true
        };
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        _powerComp = parent.TryGetComp<CompPowerPlant_Network>();
    }
}