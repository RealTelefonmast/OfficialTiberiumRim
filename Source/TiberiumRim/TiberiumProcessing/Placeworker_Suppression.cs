using System;
using System.Linq;
using TR.Components;
using TR.Comps;
using TR.Util;
using UnityEngine;
using Verse;

namespace TR.TiberiumProcessing
{
    public class PlaceWorker_Suppression : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var props = def.GetCompProperties<CompProperties_Suppression>();
            if(props == null)
                return;
            Map map = Find.CurrentMap;
            Predicate<IntVec3> pred = cell => !cell.Roofed(map) && GenSight.LineOfSight(center, cell, map);
            GenDraw.DrawFieldEdges(Enumerable.ToList(TRUtils.SectorCells(center, map, props.radius, props.angle, rot.AsAngle,false, pred)), Color.cyan);
            var otherCells = map.GetComponent<MapComponent_Suppression>().ActiveCells.ToList();
            GenDraw.DrawFieldEdges(otherCells, Color.gray);

        }
    }
}
