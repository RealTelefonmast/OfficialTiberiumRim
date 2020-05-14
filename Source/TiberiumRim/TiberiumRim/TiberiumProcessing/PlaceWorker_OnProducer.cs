using System;
using Verse;
using RimWorld;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TiberiumRim
{
    public class PlaceWorker_OnProducer : PlaceWorker
    {
        private static TiberiumProducer producer;

        private IEnumerable<TiberiumProducer> AllProducers => Find.CurrentMap.Tiberium().StructureInfo.Producers;

        private bool OnProducer()
        {
            return AllProducers.Any(p => !p.OccupiedRect().Except(CurrentCellRect).Any());
        }

        public CellRect CurrentCellRect { get; private set; }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            //if(loc.GetFirstBuilding(map) is TiberiumProducer producer && !producer.OccupiedRect().Except(CurrentCellRect).Any())
            UpdateRect(loc, loc.CellsAdjacent8Way().Select(t => t.GetFirstBuilding(map) as TiberiumProducer).First());
            if (OnProducer())
                return true;
            return "OnTiberiumProducer".Translate();
        }

        private void UpdateRect(IntVec3 loc, TiberiumProducer producer = null)
        {
            int height = 3;
            if (producer != null)
            {
                height = producer.OccupiedRect().Height;
            }

            CurrentCellRect = new CellRect(loc.x - 1, loc.z - 1, 3, height);
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var cellRect = new CellRect(center.x - 1, center.z - 1, 3, 3);
            GenDraw.DrawFieldEdges(cellRect.ToList());
            GenDraw.DrawFieldEdges(AllProducers.SelectMany(p => p.OccupiedRect()).ToList(), Color.green);
        }

        public override bool ForceAllowPlaceOver(BuildableDef other)
        {
            return other is TiberiumProducerDef def && def.forResearch;
        }
    }
}
