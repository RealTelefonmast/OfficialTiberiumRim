using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Mote_Beam : Mote
    {
        private Vector3 start;
        private Vector3 end;
        private Material drawMat;

        public void SetConnections(Vector3 start, Vector3 end, Material mat, Color color)
        {
            this.start = start;
            this.end = end;
            this.drawMat = mat;
            this.instanceColor = color;
        }

        public override void Draw()
        {
            if (drawMat == null)
            {
                return;
            }
            float alpha = this.Alpha;
            if (alpha <= 0f)
            {return;}
            Color color = instanceColor;
            color.a *= alpha;
            if(color != drawMat.color)
            {
                drawMat = MaterialPool.MatFrom((Texture2D)drawMat.mainTexture, ShaderDatabase.MoteGlow, color);
            }
            float z = (start - end).MagnitudeHorizontal();
            Vector3 pos = (start + end) / 2f;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Vector3 scale = new Vector3(1f, 1f, z);
            Quaternion quat = Quaternion.LookRotation(start - end);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, quat, scale);
            Graphics.DrawMesh(MeshPool.plane10, matrix, drawMat, 0);
        }
    }
}
