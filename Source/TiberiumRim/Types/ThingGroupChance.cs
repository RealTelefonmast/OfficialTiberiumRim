using System.Collections.Generic;
using Verse;

namespace TR.ThingSelectors;

public class ThingGroupChance
{
    public float chance = 1f;
    public List<DefFloat<ThingDef>> things = new();
}