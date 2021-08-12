using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PlaceWorker_OnProducer : PlaceWorker
    {
        private IEnumerable<TiberiumProducer> ValidProducers => Find.CurrentMap.Tiberium().NatrualTiberiumStructureInfo.ResearchProducers;

        private bool ProducerValid(TiberiumProducer producer)
        {
            return producer != null && producer.def.forResearch;
        }

        private bool Overlaps(TiberiumProducer producer, CellRect rect)
        {
            return !producer.OccupiedRect().Except(rect).Any();
        }

        private bool FitsOnProducer(TiberiumProducer producer, CellRect rect)
        {
            return ProducerValid(producer) && Overlaps(producer, rect);
        }

        //Current Resources
        private static CellRect CurrentCellRect { get; set; }
        private static TiberiumProducer Producer { get; set; }

        public override IEnumerable<TerrainAffordanceDef> DisplayAffordances()
        {
            return base.DisplayAffordances();
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            return FitsOnProducer(Producer, CurrentCellRect) ? (AcceptanceReport)true : "TR_OnTiberiumProducer".Translate();
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            CurrentCellRect = new CellRect(center.x - 1, center.z - 1, 3, 3);
            Producer = center.GetFirstBuilding(Find.CurrentMap) as TiberiumProducer;

            GenDraw.DrawFieldEdges(CurrentCellRect.ToList());
            GenDraw.DrawFieldEdges(ValidProducers.SelectMany(p => p.OccupiedRect()).ToList(), Color.green);
        }

        public override bool ForceAllowPlaceOver(BuildableDef other)
        {
            return true;
        }

    }
}
