using TeleCore.Atmosphere.Comps;
using TeleCore.PlaySettings;
using Verse;

namespace TeleCore.Mod.Patches
{
    internal class PawnCompInjection : DefInjectBase
    {
        public override void OnPawnInject(ThingDef pawnDef)
        {
            pawnDef.comps.Add(new CompProperties_PathFollowerExtra());
            pawnDef.comps.Add(new CompProperties_PawnAtmosphereTracker());
        }
    }
}
