using RimWorld;
using TeleCore.Needs;
using Verse;

namespace TeleCore.Comps;

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

public class CompProperties_Respirator : Verse.CompProperties
{
    public CompProperties_Respirator()
    {
        compClass = typeof(Comp_Respirator);
    }
}