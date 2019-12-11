using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PlaceWorker_TurretGunTop : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol)
        {
            TRThingDef trDef = def as TRThingDef;
            foreach(TurretProperties turret in trDef.turret.turrets)
            {
                Graphic graphic = GhostUtility.GhostGraphicFor(turret.turretTop.turret.Graphic, def, ghostCol);
                graphic.DrawFromDef(GenThing.TrueCenter(loc, rot, def.Size, AltitudeLayer.MetaOverlays.AltitudeFor()) + turret.drawOffset, rot, def, 0f);
            }
        }
    }
}
