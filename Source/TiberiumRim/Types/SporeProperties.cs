using System.Collections.Generic;
using Verse;

namespace TiberiumRim;

public class SporeProperties
{
    public List<WeightedThing> blossoms;
    public bool canBeGroundZero = false;
    public IntRange tickRange = new(20000, 45000);

    public TiberiumProducerDef Blossom()
    {
        return (TiberiumProducerDef)blossoms.RandomElementByWeight(x => x.weight).thing;
    }
}