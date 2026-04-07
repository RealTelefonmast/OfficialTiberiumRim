using System.Collections.Generic;
using Verse;

namespace TR.TiberiumObjects;

public class TiberiumResourceCostDef : Def
{
    public float costMultiplier = 1;
    public ThingDef resource;
    public List<TiberiumTypeCost> specificTypes;
}