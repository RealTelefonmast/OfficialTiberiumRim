using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class SporeProperties
    {
        public bool canBeGroundZero = false;
        public IntRange tickRange = new IntRange(20000, 45000);
        public List<WeightedThing> blossoms;

        public TiberiumProducerDef Blossom()
        {
            return (TiberiumProducerDef)blossoms.RandomElementByWeight(x => x.weight).thing;
        }
    }
}
