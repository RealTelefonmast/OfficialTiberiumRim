using RimWorld;
using TR.Defs;
using UnityEngine;
using Verse;

namespace TR.GameParts.PlaceWorkers;

public class PlaceWorker_TurretGunTop : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var trDef = def as TRThingDef;
        foreach (var turret in trDef.turret.turrets)
        {
            var graphic = GhostUtility.GhostGraphicFor(turret.turretTop.turret.Graphic, def, ghostCol);
            graphic.DrawFromDef(
                GenThing.TrueCenter(center, rot, def.Size, AltitudeLayer.MetaOverlays.AltitudeFor()) +
                turret.drawOffset, rot, def);
        }
    }
}