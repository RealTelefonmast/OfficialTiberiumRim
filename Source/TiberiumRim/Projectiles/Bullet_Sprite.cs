using RimWorld;
using UnityEngine;
using Verse;

namespace TR.Projectiles;

public class Bullet_Sprite : Bullet
{
    public Graphic_Sprite BulletSprite => (Graphic_Sprite)Graphic;

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
        var i = StartingTicksToImpact / BulletSprite.Count;
        //Log.Message("Ticks: " + StartingTicksToImpact + " sprites: " + BulletSprite.Count + " TickAmt: " + i);
        if (i > 0 && Find.TickManager.TicksGame % i == 0)
            BulletSprite.Next(this);
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        var mesh = MeshPool.GridPlane(BulletSprite.data.drawSize);
        Graphics.DrawMesh(mesh, DrawPos, ExactRotation, BulletSprite.CurrentGraphic(this).MatSingle, 0);
        Comps_PostDraw();
    }
}