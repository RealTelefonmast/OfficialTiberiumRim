using Verse;

namespace TeleCore.UI.SpecialSubMenu;

public class SubMenuVisibilityWorker
{
    public virtual bool IsAllowed(Def def)
    {
        if (def is BuildableDef buildable) return buildable.IsResearchFinished;
        return true;
    }
}