using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class TiberiumStructureInfo : MapInformation
    {
        public HashSet<TiberiumProducer> AllProducers = new HashSet<TiberiumProducer>();
        public HashSet<TiberiumProducer> ResearchProducers = new HashSet<TiberiumProducer>();
        public HashSet<TiberiumBlossom> Blossoms = new HashSet<TiberiumBlossom>();
        public HashSet<Building_TiberiumGeyser> Geysers = new HashSet<Building_TiberiumGeyser>();

        public TiberiumStructureInfo(Map map) : base(map) { }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public TiberiumProducer ClosestProducer(Pawn seeker)
        {
            return AllProducers.MinBy(x => x.Position.DistanceTo(seeker.Position));
        }

        public void TryRegister(TRBuilding tibobj)
        {
            switch (tibobj)
            {
                case TiberiumProducer p:
                {
                    AllProducers.Add(p);
                    if (p.def.forResearch)
                        ResearchProducers.Add(p);
                    if (p is TiberiumBlossom b)
                        Blossoms.Add(b);
                    break;
                }
                case Building_TiberiumGeyser g:
                    Geysers.Add(g);
                    break;
            }
        }

        public void Deregister(TRBuilding tibobj)
        {
            switch (tibobj)
            {
                case TiberiumProducer p:
                {
                    AllProducers.Remove(p);
                    if (p.def.forResearch)
                        ResearchProducers.Remove(p);
                    if (p is TiberiumBlossom b)
                        Blossoms.Remove(b);
                    break;
                }
                case Building_TiberiumGeyser g:
                    Geysers.Remove(g);
                    break;
            }
        }
    }
}
