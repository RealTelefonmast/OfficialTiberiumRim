using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class ThingFilterWorker_Mineable : ThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            return false;
        }

        public override bool Matches(ThingDef tDef)
        {
            return tDef?.building?.isNaturalRock ?? false;
        }
    }
}
