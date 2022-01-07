using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Mote_MuzzleFlash : TRMote
    {
        private Vector3 lookVector;

        public void SetLookDirection(Vector3 exactPos, Vector3 target)
        {
            this.exactPosition = exactPos;
            this.lookVector = target;
        }

        public override void Draw()
        {
            if (AttachedMat == null) return;
            materialProps ??= new MaterialPropertyBlock();

            //
            float alpha = Alpha;
            if (alpha <= 0f) return;
            Color color = instanceColor;
            color.a *= alpha;
            materialProps.SetColor("_Color", color);

            var diff = lookVector - exactPosition;
            Quaternion quat = Quaternion.LookRotation(diff);
            Matrix4x4 matrix = default;
            matrix.SetTRS(exactPosition, quat, this.exactScale);
            Graphics.DrawMesh(MeshPool.plane10, matrix, AttachedMat, 0, null, 0, materialProps);
        }
    }
}
