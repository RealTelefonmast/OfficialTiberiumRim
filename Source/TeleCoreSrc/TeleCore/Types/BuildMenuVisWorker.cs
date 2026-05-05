using Verse;

namespace TeleCore.Types;

public class BuildMenuVisWorker
{
    public virtual bool IsAllowed(Def def)
    {
        if (def is BuildableDef buildable) return buildable.IsResearchFinished;
        return true;
    }
}