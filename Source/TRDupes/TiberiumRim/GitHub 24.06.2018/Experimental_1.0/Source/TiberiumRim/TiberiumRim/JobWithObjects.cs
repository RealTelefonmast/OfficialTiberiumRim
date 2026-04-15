using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobWithObjects : Job, IExposable
    {
        public JobInfo jobInfo;

        public JobWithObjects()
        {
        }

        public JobWithObjects(JobDef def, LocalTargetInfo targetA, JobInfo info = null)
        {
            this.def = def;
            this.targetA = targetA;
            this.jobInfo = info;
        }

        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref jobInfo, "jobInfo");
        }
    }
}
