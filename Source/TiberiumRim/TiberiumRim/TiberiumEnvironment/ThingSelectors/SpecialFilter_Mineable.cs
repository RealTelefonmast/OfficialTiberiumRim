using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class SpecialFilter_Mineable : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            Log.Message("Checking thing " + t);
            return t.def.mineable;
        }

        public override bool AlwaysMatches(ThingDef def)
        {
            return def.mineable;
        }
    }
}
