using TR.Hediffs.TiberiumInfection;
using Verse;

namespace TR.Loading;

public class TRPawnDefInject : DefInjectBase
{
    public override void OnPawnInject(ThingDef pawnDef)
    {
        pawnDef.comps.Add(new CompProperties_TiberiumCheck());
        pawnDef.comps.Add(new CompProperties_PawnExtraDrawer());
    }
}