using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumStructureInfo
    {
        public Map map;
        public HashSet<TiberiumProducer> Producers = new HashSet<TiberiumProducer>();
        public HashSet<TiberiumGeyser> Geysers = new HashSet<TiberiumGeyser>();
        //public TiberiumProducerGrid ProducerGrid;

        public TiberiumStructureInfo(Map map)
        {
            this.map = map;
        }

        public void TryRegister(TiberiumStructure tibobj)
        {
            if (tibobj is TiberiumProducer p)
                Producers.Add(p);
            if (tibobj is TiberiumGeyser g)
                Geysers.Add(g);
        }
    }
}
