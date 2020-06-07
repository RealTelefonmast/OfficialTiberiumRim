using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public HashSet<TiberiumBlossom> Blossoms = new HashSet<TiberiumBlossom>();
        //public TiberiumProducerGrid ProducerGrid;

        public TiberiumStructureInfo(Map map)
        {
            this.map = map;
        }

        public TiberiumProducer ClosestProducer(Pawn seeker)
        {
            return Producers.MinBy(x => x.Position.DistanceTo(seeker.Position));
        }

        public void TryRegister(TRBuilding tibobj)
        {
            if (tibobj is TiberiumProducer p)
                Producers.Add(p);
            if (tibobj is TiberiumBlossom b)
                Blossoms.Add(b);
            if (tibobj is TiberiumGeyser g)
                Geysers.Add(g);
        }

        public void Deregister(TRBuilding tibobj)
        {
            if (tibobj is TiberiumProducer p)
                Producers.Remove(p);
            if (tibobj is TiberiumBlossom b)
                Blossoms.Remove(b);
            if (tibobj is TiberiumGeyser g)
                Geysers.Remove(g);
        }
    }
}
