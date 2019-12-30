using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobDriver_TResearch : JobDriver
    {
        private TResearchDef Project => TRUtils.ResearchManager().currentProj;

        private Thing ResearchThing => base.TargetThingA;

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
