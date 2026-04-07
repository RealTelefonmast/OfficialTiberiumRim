using TeleCore.Rendering.UI.SpecialSubMenu;
using TR.Defs;
using Verse;

namespace TR.Research;

public class SubMenuVisibilityWorkerTR : SubMenuVisibilityWorker
{
    public override bool IsAllowed(Def def)
    {
        if (def is TRThingDef trDef) return trDef.IsActive(out var reason);
        return base.IsAllowed(def);
    }
}