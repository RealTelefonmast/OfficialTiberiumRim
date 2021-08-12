using System;
using System.Collections.Generic;
using Verse.AI;

namespace TiberiumRim
{
    public class JobDriver_SatisfyTiberiumNeed : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            throw new NotImplementedException();
        }
    }
}
