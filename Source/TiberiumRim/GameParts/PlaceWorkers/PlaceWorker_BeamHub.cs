using System.Collections.Generic;
using TeleCore;
using UnityEngine;
using Verse;

namespace TR
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
            if (mainHub.HasConnectionInDirection(direction)) return default;
            List<IntVec3> cells = new List<IntVec3>();
            for (int i = 1; i <= length; i++)
            {
                var curCell = mainHub.Position + new IntVec3(0, 0, i).RotatedBy(direction);
                if (!curCell.InBounds(mainHub.Map)) break;
                var thing = curCell.GetFirstBuilding(mainHub.Map);
                if (thing != null && thing != mainHub && ((thing is Building || thing is Building_BeamHub) && !(thing is Building_BeamHubSegmentPart))) break;
                cells.Add(curCell);
            }
            return cells;
        }


        //TODO: Complicate problem for later
        //Need to figure how to get blueprint ghost for each cell in a row, temporary linked grid?
        private void DrawSegmentGhost(ThingDef def, IntVec3 center, Rot4 dir, int length)
        {
            if (def == null)
            {
                TRLog.ErrorOnce("Tried to draw ghost for null def", "TiberiumRim_DrawSegmentGhost".GetHashCode());
                return;
            }
            for (int i = 1; i <= length; i++)
            {
                var curCell = center + new IntVec3(0, 0, i).RotatedBy(dir);
                TDrawing.Draw(GhostUtility.GhostGraphicFor(def.graphicData.Graphic, def, Color.cyan), curCell.ToVector3Shifted(), Rot4.North, 0, null, null);
            }
        }

        private IntVec3 MinOrOther(IntVec3 trueCenter, IntVec3 center, IntVec3 other, int dir)
        {
            switch (dir)
            {
                case 0:
                    if(center.x == other.x && center.z < other.z && center.z > trueCenter.z)
                        return center;
                    break;
                case 1:
                    if(center.z == other.z && center.x < other.x && center.x > trueCenter.x)
                        return center;
                    break;
                case 2:
                    if(center.x == other.x && center.z > other.z && center.z < trueCenter.z)
                        return center;
                    break;

                case 3:
                    if(center.z == other.z && center.x > other.x && center.x < trueCenter.x)
                        return center;
                    break;
            }

            return other;
        }

        //TODO: Complicate problem for later
        //Need to figure how to get blueprint ghost for each cell in a row, temporary linked grid?
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            if (!(def is TRThingDef trDef)) return;
            foreach (var otherPart in Find.CurrentMap.listerThings.ThingsOfDef(def))
            {
                for (int i = 0; i < 4; i++)
                {
                    //TODO: Maybe make this a "raycast"
                    var otherPos = MinOrOther(otherPart.Position, center, (otherPart.Position + GenAdj.CardinalDirections[i] * trDef.beamHub.range), i);
                    if (otherPos.Standable(Find.CurrentMap))
                    {
                        GenDraw.DrawLineBetween(otherPart.DrawPos, otherPos.ToVector3Shifted(), SimpleColor.Blue);
                    }
                }
            }
            
            //
            return;
            //

            for (int i = 0; i < 4; i++)
            {
                //DrawSegmentGhost(trDef.beamHub.segmentDef, center, new Rot4(i), trDef.beamHub.range);
            }

            return;
            var hubs = Find.CurrentMap.listerThings.ThingsOfDef(trDef);
            var cells = new List<IntVec3>();
            foreach (var hub in hubs)
            {
                for (int i = 0; i < 4; i++)
                {
                    cells.AddRange(CellsForRot((Building_BeamHub)hub, new Rot4(i), trDef.beamHub.range));
                }
            }
            GenDraw.DrawFieldEdges(cells, Color.green);
        }
    }
}
