using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Rocket : ThingWithComps
    {
        public TRBuilding parent;

        public IRocketSilo Parent => (IRocketSilo)parent;

        public void Launch()
        {

        }

        public override void Tick()
        {
            base.Tick();
        }

        public virtual void Arrive()
        {

        }

        private float RocketOffsetY => 1;
        private float RocketLiftOffset => 0;

        public override void Draw()
        {
            Material nukeMat = Graphic.MatSingle;
            nukeMat.SetTextureOffset("_MainTex", new Vector2(0.25f, RocketOffsetY));
            nukeMat.SetTextureScale("_MainTex", new Vector2(0.5f, 0.5f));
            Matrix4x4 matrix4x = default(Matrix4x4);
            var pos = new Vector3(DrawPos.x, Parent.Altitude.AltitudeFor(), DrawPos.z + 2.55f);
            pos.z += RocketLiftOffset;
            matrix4x.SetTRS(pos, Quaternion.Euler(Vector3.up), new Vector3(2f, 1f, 6f));
            Graphics.DrawMesh(MeshPool.plane10, matrix4x, nukeMat, 0);
        }


    }
}
