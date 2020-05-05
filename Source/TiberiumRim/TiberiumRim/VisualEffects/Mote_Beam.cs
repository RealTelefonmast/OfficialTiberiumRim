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
        //private Vector3 finalEnd;
        private Material drawMat;

        //private bool shouldMove;

        public void SetConnections(Vector3 start, Vector3 end, Material mat, Color color)
        {
            this.start = start;
            this.end = end;
            //this.finalEnd = finalEnd;
            //shouldMove = puller != finalEnd;
            this.drawMat = mat;
            this.instanceColor = color;
        }

        public override void Tick()
        {
            base.Tick();

        }

        public override void Draw()
        {
            if (drawMat == null)
            {
                return;
            }

            float alpha = Alpha;
            /*if (shouldMove && AgeSecs >= def.mote.fadeInTime)
                end2 = Vector3.Lerp(puller, finalEnd, alpha);
            */
            Vector3 diff = end - start;
            if (alpha <= 0f) return;
            Color color = instanceColor;
            color.a *= alpha;
            if(color != drawMat.color)
            {
                drawMat = MaterialPool.MatFrom((Texture2D)drawMat.mainTexture, ShaderDatabase.MoteGlow, color);
            }
            float z = (diff).MagnitudeHorizontal();
            Vector3 pos = (start + end) / 2f;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Vector3 scale = new Vector3(1f, 1f, z);
            Quaternion quat = Quaternion.LookRotation(diff);
            Matrix4x4 matrix = default;
            matrix.SetTRS(pos, quat, scale);
            Graphics.DrawMesh(MeshPool.plane10, matrix, drawMat, 0);
        }
    }
}
