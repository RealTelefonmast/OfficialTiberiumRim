using System.Collections.Generic;
using TR.GameParts;
using Verse;

namespace TR.TiberiumObjects;

public class SporeProperties
{
    public List<DefFloat<TiberiumProducerDef>> blossoms;
    public IntRange spawnInterval = new(20000, 45000);

    public TiberiumProducerDef Blossom()
    {
        return blossoms.RandomElementByWeight(x => x.value).def;
    }
}