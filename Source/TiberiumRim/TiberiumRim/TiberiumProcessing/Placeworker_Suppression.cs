using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PlaceWorker_Suppression : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
        {
            var props = def.GetCompProperties<CompProperties_Suppression>();
            if(props == null)
                return;
            GenDraw.DrawFieldEdges(TRUtils.SectorCells(center, Find.CurrentMap, props.radius, props.angle, rot.AsAngle), Color.cyan);
        }
    }
}
