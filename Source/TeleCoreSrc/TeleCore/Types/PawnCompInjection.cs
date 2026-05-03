using TeleCore.CompProperties;
using Verse;

namespace TeleCore.Unsorted;

internal class PawnCompInjection : DefInjectBase
{
    public override void OnPawnInject(ThingDef pawnDef)
    {
        pawnDef.comps.Add(new CompProperties_PathFollowerExtra());
        pawnDef.comps.Add(new CompProperties_PawnAtmosphereTracker());
    }
}