using RimWorld;
using TeleCore.Buildings;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;

namespace TeleCore.PlaceWorkers;

public class PlaceWorker_AtTurretHub : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        if (checkingDef is ThingDef def && def.HasTurretExtension(out var extension) && extension.hub != null)
        {
            var nearbyHub = FindClosestTurretHub(def, loc, map);
            if (nearbyHub == null) return new AcceptanceReport("TR_HubTurretMissingHub".Translate());
            return true;
        }

        return true;
    }

    public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
    {
        var hub = FindClosestTurretHub(def as ThingDef, loc, Find.CurrentMap);
        hub?.AnticipateTurretAt(loc);
    }

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        if (!def.HasTurretExtension(out var extension)) return;
        if (extension.hub?.hubDef != null)
        {
            var allHubs = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(extension.hub.hubDef);
            foreach (var building in allHubs)
            {
                if (!(building is Building_TurretHubCore turretHub)) continue;
                if (!turretHub.AcceptsTurrets) continue;

                //Adjust radius because circle does not represent the actual amount of cells very well
                GenDraw.DrawCircleOutline(turretHub.DrawPos, turretHub.Extension.hub.connectRadius + 0.5f,
                    SimpleColor.Blue);
            }

            var hub = FindClosestTurretHub(def, center, Find.CurrentMap);
            if (hub == null) return;
            DrawHubConnectionGhost(center, def, hub);
        }
    }

    //
    public static Building_TurretHubCore FindClosestTurretHub(ThingDef turretDef, IntVec3 origin, Map map)
    {
        _ = turretDef.HasTurretExtension(out var extension);
        var numCells = GenRadial.NumCellsInRadius(extension.hub.connectRadius);
        for (var i = 0; i < numCells; i++)
        {
            var cell = GenRadial.RadialPattern[i] + origin;
            if (!cell.InBounds(map)) continue;
            var hub = (Building_TurretHubCore)cell.GetFirstThing(map, extension.hub.hubDef);
            if (hub != null && hub.AcceptsTurrets) return hub;
        }

        return null;
    }

    public static void DrawHubConnectionGhost(IntVec3 from, ThingDef turretDef, Building_TurretHubCore hubCore)
    {
        var cableMat = MaterialPool.MatFrom(hubCore.Extension.hub.cableTexturePath, ShaderTypeDefOf.EdgeDetect.Shader);

        var placingTurret =
            GenThing.TrueCenter(from, Rot4.North, turretDef.size, AltitudeLayer.MapDataOverlay.AltitudeFor());
        var hubPos = hubCore.TrueCenter();
        hubPos.y = AltitudeLayer.MapDataOverlay.AltitudeFor();

        var meanPosBetween = (placingTurret + hubPos) / 2f;
        var vectorBetween = hubPos - placingTurret;
        var s = new Vector3(1f, 1f, vectorBetween.MagnitudeHorizontal());
        var q = Quaternion.LookRotation(hubPos - placingTurret);
        Matrix4x4 matrix = default;
        matrix.SetTRS(meanPosBetween, q, s);
        Graphics.DrawMesh(MeshPool.plane10, matrix, cableMat, 0);
    }
}