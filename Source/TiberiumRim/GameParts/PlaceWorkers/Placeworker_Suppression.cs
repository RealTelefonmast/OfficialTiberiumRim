using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim
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
            GenDraw.DrawFieldEdges(TRUtils.SectorCells(center, map, props.radius, props.angle, rot.AsAngle,false, pred).ToList(), Color.blue);
            var coveredCells = map.Tiberium().SuppressionInfo.CoveredCells.ToList();
            var suppressedCells = map.Tiberium().SuppressionInfo.SuppressedCells.ToList();
            GenDraw.DrawFieldEdges(coveredCells, Color.gray);
            GenDraw.DrawFieldEdges(suppressedCells, Color.cyan);

        }
    }
}
