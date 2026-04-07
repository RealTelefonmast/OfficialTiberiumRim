using System;
using TR.TiberiumEnvironment.ThingSelectors;
using Verse;

namespace TR.Defs;

public class ThingFilterDef : Def
{
    public ThingFilter filter;
    public Type worker;

    private ThingFilterWorker workerInt;

    private ThingFilterWorker Worker
    {
        get { return workerInt ??= (ThingFilterWorker)Activator.CreateInstance(worker); }
    }

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        filter?.ResolveReferences();
    }

    public bool Allows(ThingDef thing)
    {
        if (worker != null)
            return Worker.Matches(thing);
        return filter.Allows(thing);
    }
}