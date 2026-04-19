using System.Collections.Generic;
using TiberiumRim;
using Verse;

namespace TR;

public class TiberiumResourceCostDef : Def
{
    public float costMultiplier = 1;
    public ThingDef resource;
    public List<TiberiumTypeCost> specificTypes;
}