using UnityEngine;
using Verse;

namespace TeleCore.RWExtended.Verbs;

/// <summary>
/// A special projectile type that causes damaging effects as it moves towards a target
/// </summary>
public class Projectile_Wanderer : Projectile
{
    public override void Tick()
    {
        base.Tick();
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
    }
}