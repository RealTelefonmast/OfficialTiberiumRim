﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PlaceWorker_BeamHub : PlaceWorker
    {
        /*
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null,
            Thing thing = null)
        {
            return base.AllowsPlacing(checkingDef, loc, rot, map, thingToIgnore, thing);
        }
        */

        private List<IntVec3> CellsForRot(Building_BeamHub mainHub, Rot4 direction, int length)
        {
            List<IntVec3> cells = new List<IntVec3>();
            for (int i = 1; i <= length; i++)
            {
                var curCell = mainHub.Position + new IntVec3(0, 0, i).RotatedBy(direction);
                if (!curCell.InBounds(mainHub.Map)) break;
                var thing = curCell.GetFirstBuilding(mainHub.Map);
                if (thing != null && thing != mainHub && (thing is Building || thing is Building_BeamHub || thing is Building_BeamHubSegmentPart)) break;
                cells.Add(curCell);
            }
            return cells;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            if (def is TRThingDef trDef)
            {
                var hubs = Find.CurrentMap.listerThings.ThingsOfDef(trDef);
                var cells = new List<IntVec3>();
                foreach (var hub in hubs)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        cells.AddRange(CellsForRot((Building_BeamHub)hub, new Rot4(i), trDef.beamHub.range));
                    }
                }
                GenDraw.DrawFieldEdges(cells, Color.grey);
            }
            base.DrawGhost(def, center, rot, ghostCol, thing);
        }
    }
}
