using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Projectile_ThrownFlame : Projectile
    {
        public Graphic_Sprite GraphicSprite => Graphic as Graphic_Sprite;

        private int BaseInterval => (int)(StartingTicksToImpact / GraphicSprite.Count);
        public int AdjustedInterval => ticksToImpact / GraphicSprite.RemainingFor(this);


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            GraphicSprite.AddIndex(this);
            exactScale = new Vector3(0.25f, 1, 0.25f);
        }

        public Building_FlameTurret FlameTurret => Launcher as Building_FlameTurret;

        private static float fadeOutBegin = 0.6f;
        public override void Tick()
        {
            base.Tick();
            var tick = Find.TickManager.TicksGame;
            float timeVal = 1f - (ticksToImpact / StartingTicksToImpact);

            //Sprite TIck
            if (tick % BaseInterval == 0 && Spawned)
            {
                var val = Mathf.Lerp(0.2f, MaxSize, timeVal);
                exactScale = new Vector3(val, 1, val);
                GraphicSprite.Next(this);
            }

            //Opacity
            if (timeVal > fadeOutBegin)
                opacity = Mathf.Lerp(1f, 0f, 1f - ((1f - timeVal) / (1f - fadeOutBegin)));
        }

        public float opacity = 1f;
        public Vector3 exactScale = Vector3.one;

        public int Interval
        {
            get
            {
                Log.Message("Interval" + fadeOutBegin);
                if (PositionPct > fadeOutBegin)
                {
                    return AdjustedInterval;
                }
                return BaseInterval * 4;
            }
        }


        public float MaxSize => Mathf.Lerp(0.5f, 1.3f, RangePct);
        private float PositionPct => ExactPosition.ToIntVec3().DistanceTo(intendedTarget.Cell) / Launcher.Position.DistanceTo(intendedTarget.Cell);
        private float RangePct => intendedTarget.Cell.DistanceTo(this.Launcher.Position) / FlameTurret.MainGun.VerbProps.range;

        public override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
        }

        protected MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        public override void Draw()
        {

            Color color = Color.white;
            color.a *= opacity;

            Matrix4x4 matrix = default;
            Vector3 exactScale2 = exactScale;
            exactScale2.x *= Graphic.data.drawSize.x;
            exactScale2.z *= Graphic.data.drawSize.y;
            matrix.SetTRS(DrawPos, ExactRotation, exactScale2);
            Material matSingle = GraphicSprite.CurrentGraphic(this).MatSingle;
            //Overlays.DrawMesh(MeshPool.plane10, matrix, matSingle, 0, null, 0);

            propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
            Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, 0, null, 0, propertyBlock);

            //Mesh mesh = MeshPool.GridPlane(GraphicSprite.data.drawSize);
            //GraphicSprite.DrawWorker(DrawPos, );

            //Overlays.DrawMesh(mesh, DrawPos, ExactRotation, GraphicSprite.CurrentGraphic(this).MatSingle, 0);
            //Comps_PostDraw();
        }
    }
}
