using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class GraphicDrawInfo
    {
        public Vector3 drawPos;
        public Vector2 drawSize;
        public Material drawMat;
        public Mesh drawMesh;
        public float rotation = 0;
        public bool flipUV = false;

        public GraphicDrawInfo(Graphic g, Vector3 rootPos, Rot4 rot, ExtendedGraphicData exData, ThingDef def = null)
        {
            drawMat = g.MatAt(rot);

            //DrawPos
            drawPos = rootPos;
            if ((exData?.alignToBottom ?? false) && def != null)
            {
                drawPos.z += AlignToBottomOffset(def, g.drawSize);
            }

            drawPos += exData?.drawOffset ?? Vector3.zero;

            //DrawSize
            drawSize = g.drawSize;
            if (g.ShouldDrawRotated)
            {
                flipUV = false;
            }
            else
            {
                if (rot.IsHorizontal && (exData?.rotateDrawSize ?? true))
                {
                    drawSize = drawSize.Rotated();
                }
                flipUV = /*!g.ShouldDrawRotated &&*/ ((rot == Rot4.West && g.WestFlipped) || (rot == Rot4.East && g.EastFlipped));
            }
            drawMesh = flipUV ? MeshPool.GridPlaneFlip(drawSize) : MeshPool.GridPlane(drawSize);
            rotation = AngleFromRotFor(g, rot, exData?.drawRotated ?? true);
        }

        private float AngleFromRotFor(Graphic g, Rot4 rot, bool drawRotated)
        {
            if (!drawRotated || !g.ShouldDrawRotated) return 0f;

            float num = rot.AsAngle;
            num += g.DrawRotatedExtraAngleOffset;
            if ((rot == Rot4.West && g.WestFlipped) || (rot == Rot4.East && g.EastFlipped))
            {
                num += 180f;
            }
            return num;
        }

        private float AlignToBottomOffset(ThingDef def, Vector2 drawSize)
        {
            float height = drawSize.y;
            float selectHeight = def.size.z;
            float diff = height - selectHeight;
            return diff / 2;
        }
    }
}
