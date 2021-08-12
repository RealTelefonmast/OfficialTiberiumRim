using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PlaceWorker_TurretGunTop : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            TRThingDef trDef = def as TRThingDef;
            foreach (TurretProperties turret in trDef.turret.turrets)
            {
                Graphic graphic = GhostUtility.GhostGraphicFor(turret.turretTop.turret.Graphic, def, ghostCol);
                graphic.DrawFromDef(GenThing.TrueCenter(center, rot, def.Size, AltitudeLayer.MetaOverlays.AltitudeFor()) + turret.drawOffset, rot, def, 0f);
            }
        }
    }
}
