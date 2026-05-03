using TR.TiberiumInfection;
using Verse;

namespace TR;

public class TRPawnDefInject : DefInjectBase
{
    public override void OnPawnInject(ThingDef pawnDef)
    {
        pawnDef.comps.Add(new CompProperties_TiberiumCheck());
        pawnDef.comps.Add(new TeleCore.Rendering.CompProperties_PawnExtraDrawer());
    }
}