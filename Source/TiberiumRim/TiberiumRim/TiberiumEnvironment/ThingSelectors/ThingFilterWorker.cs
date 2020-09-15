using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
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
}
