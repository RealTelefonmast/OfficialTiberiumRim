using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class ThingGroupChance
    {
        public List<DefFloat<ThingDef>> things = new ();
        public float chance = 1f;
    }
}
