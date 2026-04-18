using RimWorld;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;

namespace TeleCore.PlaceWorkers;

/// <summary>
///     Renders all turrets defined in TeleDefExtension ontop of the placable thing.
/// </summary>
public class PlaceWorker_TurretGunTop : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var extension = def.TurretExtension();
        foreach (var turret in extension.turrets)
        {
            var top = turret.turretTop;
            var graphic = GhostUtility.GhostGraphicFor(top.topGraphic.Graphic, def, ghostCol);
            var offset = GenThing.TrueCenter(center, rot, def.Size, AltitudeLayer.MetaOverlays.AltitudeFor()) + turret.turretOffset;
            graphic.DrawFromDef(offset, rot, def);
            if (top.barrels != null)
            {
                foreach (var barrel in top.barrels)
                {
                    var barrelGraphic = GhostUtility.GhostGraphicFor(barrel.graphic.Graphic, def, ghostCol);
                    barrelGraphic.DrawFromDef(offset + barrel.barrelOffset, rot, def);
                }
            }
        }
    }
}