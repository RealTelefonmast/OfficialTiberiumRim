using TeleCore.Types.Abstracts;
using Verse;

namespace TeleCore.Types;

public class OxygenDefInjection : DefInjectBase
{
    public override void OnPawnInject(ThingDef pawnDef)
    {
        if (pawnDef.race != null)
        {
        }
    }
}