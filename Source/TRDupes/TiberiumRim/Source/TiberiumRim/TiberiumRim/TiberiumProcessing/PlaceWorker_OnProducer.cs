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

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {

            if(producer != null)
                return true;
            return "OnTiberiumProducer".Translate();
        }

        public void GetRects(BuildableDef checkDef, IntVec3 loc, Map map, out CellRect checkingRect, out CellRect thingRect, out TiberiumProducer thing)
        {
            thingRect = new CellRect();
            checkingRect = new CellRect(loc.x - 1, loc.z - 1, 3, 3);
            thing = (TiberiumProducer)checkingRect.GetAnyThingIn<TiberiumProducer>(map);
            if (thing != null)
            {
                thingRect = thing.OccupiedRect();
                if (thingRect.Height <= 2)
                {
                    checkingRect.maxZ -= 1;
                    if (thingRect.Height <= 1)
                        checkingRect.maxZ -= 1;
                }
                if (thingRect.Cells.ToList().All(checkingRect.Cells.ToList().Contains))
                {
                    producer = thing;
                    return;
                }
            }
            producer = null;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            //GenDraw.DrawFieldEdges(new List<IntVec3>() { center }, Color.red);
            GetRects(def, center, Find.CurrentMap, out CellRect checkingRect, out CellRect thingRect, out TiberiumProducer prod);
            if (prod != null)
            {
                GenDraw.DrawFieldEdges(thingRect.Cells.ToList(), Color.green);
            }
            GenDraw.DrawFieldEdges(checkingRect.Cells.ToList());
        }

        public override bool ForceAllowPlaceOver(BuildableDef other)
        {
            return (other == producer?.def) && (bool)producer?.def.forResearch;
        }
    }
}
