using TeleCore.Rendering.UI.SpecialSubMenu;
using TR.Defs;
using Verse;

namespace TR;

public class SubMenuVisibilityWorkerTR : SubMenuVisibilityWorker
{
    public override bool IsAllowed(Def def)
    {
        if (def is TRThingDef trDef) return trDef.IsActive(out var reason);

        if (def is TerrainDef terrDef)
            return terrDef.IsResearchFinished && terrDef.HasResearchExtension(out var research) && research.IsFinished;
        return base.IsAllowed(def);
    }
}