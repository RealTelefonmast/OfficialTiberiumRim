using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class AttackSatellite : TRWorldObject
    {
        public int tileDest;

        private float elevation   = 10f;
        private float speed     = 1;
        private float travelPct = 0;
        private int travelTicks;
        private Vector3 startPos;
        private Vector3 endPos;

        private static readonly Material TargetBar = MaterialPool.MatFrom("Buildings/GDI/Defense/IonCannon/ASAT_ION_TargetBar", ShaderDatabase.Cutout);
        private static readonly Material TargetTile = MaterialPool.MatFrom("Buildings/GDI/Defense/IonCannon/TargetTile", ShaderDatabase.Cutout);
        private static readonly Material TargetTileUnsel = MaterialPool.MatFrom("Buildings/GDI/Defense/IonCannon/TargetTile_Unsel", ShaderDatabase.Cutout);
        private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            TRUtils.Tiberium().SatelliteInfo.AttackSatelliteNetwork.RegisterNew(this);
            SetDestination(Tile);
        }

        public override void Tick()
        {
            base.Tick();
            if(Find.WorldSelector.IsSelected(this))
                Log.Message("Is over: " + OverTile.biome.defName, true);
            if (!ShouldMove) return;
            travelPct += 1f / travelTicks;
            if(travelPct >= 1)
                Arrive();
        }

        public void SetDestination(int tile)
        {
            if (tile < 0) return;
            tileDest = tile;
            startPos = !ShouldMove ? Find.WorldGrid.GetTileCenter(Tile) : MovingPos;
            endPos = Find.WorldGrid.GetTileCenter(tileDest);
            travelPct = 0;

            travelTicks = (int)Find.WorldGrid.ApproxDistanceInTiles(GenMath.SphericalDistance(startPos.normalized, endPos.normalized)) * 60;
        }

        protected virtual void Arrive()
        {
            Tile = tileDest;
            SetDestination(Tile);
        }

        public Tile OverTile
        {
            get
            {
                int i = Tile;
                Ray ray = new Ray(Vector3.zero, DrawPos);
                
                int worldLayerMask = WorldCameraManager.WorldLayerMask;
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1500f, worldLayerMask))
                {
                    i = Find.World.renderer.GetTileIDFromRayHit(hit);
                }
                return Find.WorldGrid[i];
            }
        }

        protected Vector3 MovingPos
        {
            get { return Vector3.Slerp(startPos, endPos, travelPct); }
        }

        public override Vector3 DrawPos
        {
            get
            {
                Vector3 v3 = MovingPos;
                v3.y += elevation;
                return v3;
            }
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
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawPos, Quaternion.Euler(-90f, 0, 0), -new Vector3(2, 1f, beamHeight));
            Graphics.DrawMesh(MeshPool.plane10, matrix, TargetBar, WorldCameraManager.WorldLayer, null, 0, propertyBlock);

            WorldRendererUtility.DrawQuadTangentialToPlanet(MovingPos, Find.WorldGrid.averageTileSize * 0.75f, 0.02f, TargetTile, false, false, propertyBlock);
        }

        public bool ShouldMove => Tile != tileDest;
    }
}
