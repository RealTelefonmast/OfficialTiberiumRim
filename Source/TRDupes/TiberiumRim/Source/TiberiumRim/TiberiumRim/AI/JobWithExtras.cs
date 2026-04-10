using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
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
}
