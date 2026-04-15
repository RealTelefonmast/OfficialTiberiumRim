using TeleCore.RWExtended.ThingClasses;

namespace TR;

public class SuppressionTower : FXThing
{
    public Comp_Suppression Comp => GetComp<Comp_Suppression>();

    private void SendWave()
    {
    }
}

