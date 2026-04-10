using System.Collections.Generic;
using Verse;

namespace TR;

public class SporeProperties
{
    public List<DefFloat<TiberiumProducerDef>> blossoms;
    public IntRange spawnInterval = new(20000, 45000);

    public TiberiumProducerDef Blossom()
    {
        return blossoms.RandomElementByWeight(x => x.value).def;
    }
}