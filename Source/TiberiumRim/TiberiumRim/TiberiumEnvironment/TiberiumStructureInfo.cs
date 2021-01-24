using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumStructureInfo : MapInformation
    {
        public HashSet<TiberiumProducer> AllProducers = new HashSet<TiberiumProducer>();
        public HashSet<TiberiumProducer> ResearchProducers = new HashSet<TiberiumProducer>();
        public HashSet<TiberiumBlossom> Blossoms = new HashSet<TiberiumBlossom>();
        public HashSet<TiberiumGeyser> Geysers = new HashSet<TiberiumGeyser>();
        //public TiberiumProducerGrid ProducerGrid;

        public TiberiumStructureInfo(Map map) : base(map) { }

        public TiberiumProducer ClosestProducer(Pawn seeker)
        {
            return AllProducers.MinBy(x => x.Position.DistanceTo(seeker.Position));
        }

        public void TryRegister(TRBuilding tibobj)
        {
            if (tibobj is TiberiumProducer p)
            {
                AllProducers.Add(p);
                if (p.def.forResearch)
                    ResearchProducers.Add(p);
                if (p is TiberiumBlossom b)
                    Blossoms.Add(b);
            }
            if (tibobj is TiberiumGeyser g)
                Geysers.Add(g);
        }

        public void Deregister(TRBuilding tibobj)
        {
            if (tibobj is TiberiumProducer p)
            {
                AllProducers.Remove(p);
                if (p.def.forResearch)
                    ResearchProducers.Remove(p);
            }

            if (tibobj is TiberiumBlossom b)
                Blossoms.Remove(b);
            if (tibobj is TiberiumGeyser g)
                Geysers.Remove(g);
        }
    }
}
