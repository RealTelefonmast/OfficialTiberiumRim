using System.Collections.Generic;
using Verse;

namespace TR
{
    public class SporeProperties
    {
        public IntRange spawnInterval = new IntRange(20000, 45000);
        public List<DefFloat<TiberiumProducerDef>> blossoms;

        public TiberiumProducerDef Blossom()
        {
            return blossoms.RandomElementByWeight(x => x.value).def;
        }
    }
}
