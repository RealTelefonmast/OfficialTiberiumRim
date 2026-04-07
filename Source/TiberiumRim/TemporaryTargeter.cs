using TR.ThingData;
using TR.Util;
using UnityEngine;

namespace TR;

public class TemporaryTargeter : TRBuildingPrototype
{
    public Material mat;
    public float size;

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        TRUtils.DrawTargeter(Position, mat, size);
        base.DrawAt(drawLoc, flip);
    }
}