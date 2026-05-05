using TeleCore.CompProperties;
using TeleCore.Types.Abstracts;
using Verse;

namespace TeleCore.Types;

internal class PawnCompInjection : DefInjectBase
{
    public override void OnPawnInject(ThingDef pawnDef)
    {
        pawnDef.comps.Add(new CompProperties_PathFollowerExtra());
        pawnDef.comps.Add(new CompProperties_PawnAtmosphereTracker());
    }
}