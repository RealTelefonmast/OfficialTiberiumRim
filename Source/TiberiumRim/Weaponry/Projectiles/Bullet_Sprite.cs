using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Bullet_Sprite : Bullet
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            BulletSprite.AddIndex(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            BulletSprite.RemoveIndex(this);
        }

        public override void Tick()
        {
            base.Tick();
            float i = StartingTicksToImpact / BulletSprite.Count;
            //Log.Message("Ticks: " + StartingTicksToImpact + " sprites: " + BulletSprite.Count + " TickAmt: " + i);
            if (i > 0 && Find.TickManager.TicksGame % i == 0)
                BulletSprite.Next(this);
        }

        public Graphic_Sprite BulletSprite => (Graphic_Sprite) Graphic;

        public override void Draw()
        {
            Mesh mesh = MeshPool.GridPlane(BulletSprite.data.drawSize);
            Graphics.DrawMesh(mesh, DrawPos, ExactRotation, BulletSprite.CurrentGraphic(this).MatSingle, 0);
            Comps_PostDraw();
        }
    }
}
