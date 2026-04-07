using System.Collections.Generic;
using TR.GameParts;
using Verse;

namespace TR.TiberiumEnvironment.ThingSelectors;

public class ThingGroupChance
{
    public float chance = 1f;
    public List<DefFloat<ThingDef>> things = new();
}