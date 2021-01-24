using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class TargetProperties
    {
        public Type targetType;
        public List<ThingDef> targetDefs;
        public string groupLabel;

        public bool Accepts(Thing thing)
        {
            if (targetType != null && thing.def.thingClass == targetType)
            {
                return true;
            }
            if (targetDefs.NullOrEmpty()) return false;
            return targetDefs.Contains(thing.def);
        }
    }
}
