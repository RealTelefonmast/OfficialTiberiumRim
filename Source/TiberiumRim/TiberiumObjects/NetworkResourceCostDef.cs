using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class NetworkResourceCostDef : Def
    {
        public ThingDef resource;
        public float costMultiplier = 1;
        public List<NetworkCostValue> specificTypes;
    }
}
