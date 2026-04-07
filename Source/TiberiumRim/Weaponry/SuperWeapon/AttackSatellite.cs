using RimWorld.Planet;
using TR.GameParts;
using TR.ThingData;
using TR.Util;
using UnityEngine;
using Verse;

namespace TR.Weaponry.SuperWeapon;

[StaticConstructorOnStartup]
public class AttackSatellite : TRWorldObject
{
    private static readonly Material TargetBar =
        MaterialPool.MatFrom("Buildings/GDI/Defense/IonCannon/ASAT_ION_TargetBar", ShaderDatabase.Cutout);

    private static readonly Material TargetTile =
        MaterialPool.MatFrom("Buildings/GDI/Defense/IonCannon/TargetTile", ShaderDatabase.Cutout);

    private static readonly Material TargetTileUnsel =
        MaterialPool.MatFrom("Buildings/GDI/Defense/IonCannon/TargetTile_Unsel", ShaderDatabase.Cutout);

    private ISuperWeapon _swep;
    private static readonly MaterialPropertyBlock propertyBlock = new();
    private readonly float elevation = 10f;
    private Vector3 endPos;
    private float speed = 1;
    private Vector3 startPos;
    public int tileDest;
    private float travelPct;
    private int travelTicks;

    public bool ShouldMove => Tile != tileDest;

    public Tile OverTile
    {
        get
        {
            int i = Tile;
            var ray = new Ray(Vector3.zero, DrawPos);

            var worldLayerMask = WorldCameraManager.WorldLayerMask;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1500f, worldLayerMask)) i = Find.World.renderer.GetTileFromRayHit(hit);
            return Find.WorldGrid[i];
        }
    }

    protected Vector3 MovingPos => Vector3.Slerp(startPos, endPos, travelPct);

    public override Vector3 DrawPos
    {
        get
        {
            Vector3 v3 = MovingPos;
            v3.y += elevation;
            return v3;
        }
    }

    public override void SpawnSetup()
    {
        base.SpawnSetup();
        TRUtils.Tiberium().SatelliteInfo.AttackSatelliteNetwork.RegisterNew(this);
        SetDestination(Tile);
    }

    public override void Tick()
    {
        base.Tick();
        if (Find.WorldSelector.IsSelected(this))
            Log.Message("Is over: " + OverTile.biome.defName, true);
        if (!ShouldMove) return;
        travelPct += 1f / travelTicks;
        if (travelPct >= 1)
            Arrive();
    }

    public void SetDestination(int tile)
    {
        if (tile < 0) return;
        tileDest = tile;
        startPos = !ShouldMove ? Find.WorldGrid.GetTileCenter(Tile) : MovingPos;
        endPos = Find.WorldGrid.GetTileCenter(tileDest);
        travelPct = 0;
        travelTicks =
            (int)Find.WorldGrid.ApproxDistanceInTiles(GenMath.SphericalDistance(startPos.normalized,
                endPos.normalized)) * 60;
    }

    protected virtual void Arrive()
    {
        Tile = tileDest;
        SetDestination(Tile);
    }

    public override void Draw()
    {
        base.Draw();
        if (!Find.WorldSelector.IsSelected(this))
        {
            WorldRendererUtility.DrawQuadTangentialToPlanet(MovingPos, Find.WorldGrid.averageTileSize * 0.75f, 0.02f, TargetTileUnsel, false, false, propertyBlock);
            return;
        }

        float beamHeight = elevation;
        Vector3 drawPos = DrawPos;
        drawPos.y = DrawPos.y - (beamHeight * 0.5f);

        var vec = Quaternion.FromToRotation(MovingPos, DrawPos).eulerAngles;
        propertyBlock.SetColor(ShaderPropertyIDs.Color, Color.white);
        var matrix = default(Matrix4x4);
        matrix.SetTRS(drawPos, Quaternion.Euler(-90f, 0, 0), -new Vector3(2, 1f, beamHeight));
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, TargetBar, WorldCameraManager.WorldLayer, null, 0, propertyBlock);

        WorldRendererUtility.DrawQuadTangentialToPlanet(MovingPos, Find.WorldGrid.averageTileSize * 0.75f, 0.02f,
            TargetTile, false, false, propertyBlock);
    }
}
