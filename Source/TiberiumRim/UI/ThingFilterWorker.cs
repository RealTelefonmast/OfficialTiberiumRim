using Verse;

namespace TR.ThingSelectors;

public abstract class ThingFilterWorker
{
    public abstract bool Matches(Thing t);

    public abstract bool Matches(ThingDef tDef);

    public virtual bool AlwaysMatches(ThingDef def)
    {
        return false;
    }

    public virtual bool CanEverMatch(ThingDef def)
    {
        return true;
    }
}