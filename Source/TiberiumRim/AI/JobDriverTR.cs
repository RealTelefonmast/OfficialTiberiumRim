using System;
using System.Collections.Generic;
using Verse.AI;

namespace TiberiumRim
{
    public abstract class JobDriverTR : JobDriver
    {
        private List<Action<JobCondition>> finishActionsConditional = new ();

        public void CleanupPostfix(JobCondition condition)
        {
            foreach (var action in finishActionsConditional)
            {
                action.Invoke(condition);
            }
        }

        public void AddConditionalFinishAction(Action<JobCondition> action)
        {
            finishActionsConditional.Add(action);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            throw new NotImplementedException();
        }
    }
}
