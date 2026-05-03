using Verse;

namespace TeleCore.Unsorted;

public class SubMenuVisibilityWorker
{
    public virtual bool IsAllowed(Def def)
    {
        if (def is BuildableDef buildable) return buildable.IsResearchFinished;
        return true;
    }
}