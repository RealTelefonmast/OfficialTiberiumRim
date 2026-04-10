using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TR;

public class JobWithExtras : Job
{
    public List<Hediff> hediffs;

    public JobWithExtras(JobDef def) : base(def, null)
    {
    }

    public JobWithExtras(JobDef def, LocalTargetInfo targetA) : base(def, targetA, null)
    {
    }
}