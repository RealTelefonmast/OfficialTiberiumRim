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

        public bool stopBlossom = false;

        public TiberiumStructureInfo(Map map)
        {
            this.map = map;
        }

        public IntVec3 GetBlossomDestination()
        {
            if (stopBlossom) return IntVec3.Invalid;

            Predicate<IntVec3> pred = c => Blossoms.All(b => b.Position.DistanceTo(c) >= b.radius) && c.SupportsBlossom(map);
            if (!CellFinderLoose.TryGetRandomCellWith(pred, map, 999, out IntVec3 dest))
            {
                stopBlossom = true;
                return IntVec3.Invalid;
            }
            return dest;
        }

        public void Notify_BlossomGone()
        {
            stopBlossom = false;
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
    }
}
