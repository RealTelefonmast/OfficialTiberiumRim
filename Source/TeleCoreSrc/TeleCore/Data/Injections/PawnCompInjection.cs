using TeleCore.Comps;
using TeleCore.PlaySettings;
using Verse;

namespace TeleCore.Injections;

internal class PawnCompInjection : DefInjectBase
{
    public override void OnPawnInject(ThingDef pawnDef)
    {
        pawnDef.comps.Add(new CompProperties_PathFollowerExtra());
        pawnDef.comps.Add(new CompProperties_PawnAtmosphereTracker());
    }
}