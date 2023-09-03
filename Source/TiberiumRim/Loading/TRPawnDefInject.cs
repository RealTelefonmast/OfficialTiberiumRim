using TeleCore;
using Verse;

namespace TR
{
    public class TRPawnDefInject : DefInjectBase
    {
        public override void OnPawnInject(ThingDef pawnDef)
        {
            pawnDef.comps.Add(new CompProperties_TiberiumCheck());
            pawnDef.comps.Add(new CompProperties_PawnExtraDrawer());
            pawnDef.comps.Add(new CompProperties_CrystalDrawer());
        }
    }
}
