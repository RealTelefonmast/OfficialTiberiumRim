using System;
using Verse;
using RimWorld;
using UnityEngine;
using System.Linq;

namespace TiberiumRim
{
    public class PlaceWorker_OnProducer : PlaceWorker
    {
        private TiberiumProducer producer = null;
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            GetRects(checkingDef, loc, map, out CellRect checkingRect, out CellRect thingRect, out Thing thing);
            if (thing != null)
            {
                if (thingRect.Cells.ToList().All(checkingRect.ContractedBy(1).Cells.ToList().Contains))
                {
                    producer = (TiberiumProducer)thing;
                    return true;
                }
            }
            return "OnTiberiumProducer".Translate();
        }

        public void GetRects(BuildableDef checkDef, IntVec3 loc, Map map, out CellRect checkingRect, out CellRect thingRect, out Thing thing)
        {
            thingRect = new CellRect();
            IntVec2 size = checkDef.Size;
            checkingRect = new CellRect(loc.x - size.x / 2, loc.z - size.z / 2, size.x, size.z);
            thing = checkingRect.GetAnyThingIn(typeof(TiberiumProducer), Find.CurrentMap);
            if (thing != null)
            {
                thingRect = thing.OccupiedRect();
                if (thingRect.Width <= 2)
                {
                    checkingRect.maxZ -= 1;
                    if (thingRect.Width <= 1)
                    {
                        checkingRect.maxZ -= 1;
                    }
                }
            }
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
        {
            GetRects(def, center, Find.CurrentMap, out CellRect checkingRect, out CellRect thingRect, out Thing thing);
            if (thing != null)
            {
                GenDraw.DrawFieldEdges(thingRect.Cells.ToList(), Color.green);
            }
            GenDraw.DrawFieldEdges(checkingRect.ContractedBy(1).Cells.ToList());
            base.DrawGhost(def, center, rot, ghostCol);
        }

        public override bool ForceAllowPlaceOver(BuildableDef other)
        {
            return other == producer.def;
        }
    }
}
