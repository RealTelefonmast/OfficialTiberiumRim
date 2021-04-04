using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class TiberiumResourceCostDef : Def
    {
        public ThingDef resource;
        public float costMultiplier = 1;
        public List<TiberiumTypeCost> specificTypes;
    }
}
