using System.Collections.Generic;
using Verse;

namespace TR
{
    public class ThingGroupChance
    {
        public List<DefFloat<ThingDef>> things = new ();
        public float chance = 1f;
    }
}
