using TeleCore;
using Verse;

namespace TR;

public class SubMenuVisibilityWorkerTR : SubMenuVisibilityWorker
{
    public override bool IsAllowed(Def def)
    {
        if (def is TRThingDef trDef)
        {
            return trDef.IsActive(out var reason);
        }
        return base.IsAllowed(def);
    }
}