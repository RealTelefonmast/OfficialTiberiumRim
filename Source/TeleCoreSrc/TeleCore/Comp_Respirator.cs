using RimWorld;
using TeleCore.Atmosphere.Health;
using Verse;

namespace TeleCore;

/// <summary>
///     Used to provide breathable gas via equipment - should usually be applied on an apparel.
/// </summary>
public class Comp_Respirator : ThingComp
{
    private CompRefuelable _fuelComp;

    public CompProperties_Respirator Props => (CompProperties_Respirator)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
    }

    public override void CompTickRare()
    {
        base.CompTickRare();
    }

    public void Consume_ByNeed(Need_Respiration consumer)
    {
    }
}

public class CompProperties_Respirator : CompProperties
{
    public CompProperties_Respirator()
    {
        compClass = typeof(Comp_Respirator);
    }
}