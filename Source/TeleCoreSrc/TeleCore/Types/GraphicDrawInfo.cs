using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class GraphicDrawInfo
{
    public Material drawMat;
    public Mesh drawMesh;
    public Vector3 drawPos;
    public Vector2 drawSize;
    public bool flipUV;
    public float rotation;

    public GraphicDrawInfo(Graphic g, Vector3 rootPos, Rot4 rot, ExtendedGraphicData exData, ThingDef def = null,
        Thing parent = null)
    {
        drawMat = g.MatAt(rot);

        //DrawPos
        drawPos = rootPos;
        if ((exData?.alignToBottom ?? false) && def != null) drawPos.z += AlignToBottomOffset(def, g.drawSize);
        drawPos += exData?.drawOffset ?? Vector3.zero;
        //DrawSize
        drawSize = g.drawSize;
        var drawRotated = exData?.drawRotatedOverride ?? g.ShouldDrawRotated;
        if (drawRotated)
        {
            flipUV = false;
        }
        else
        {
            if (rot.IsHorizontal && (exData?.rotateDrawSize ?? true)) drawSize = drawSize.Rotated();
            flipUV = /*!g.ShouldDrawRotated &&*/
                (rot == Rot4.West && g.WestFlipped) || (rot == Rot4.East && g.EastFlipped);
        }

        drawMesh = flipUV ? MeshPool.GridPlaneFlip(drawSize) : MeshPool.GridPlane(drawSize);
        rotation = AngleFromRotFor(g, rot, drawRotated);
    }

    private float AngleFromRotFor(Graphic g, Rot4 rot, bool drawRotated)
    {
        if (!drawRotated) return 0f;

        var num = rot.AsAngle;
        num += g.DrawRotatedExtraAngleOffset;
        if ((rot == Rot4.West && g.WestFlipped) || (rot == Rot4.East && g.EastFlipped)) num += 180f;
        return num;
    }

    private float AlignToBottomOffset(ThingDef def, Vector2 drawSize)
    {
        var height = drawSize.y;
        float selectHeight = def.size.z;
        var diff = height - selectHeight;
        return diff / 2;
    }
}