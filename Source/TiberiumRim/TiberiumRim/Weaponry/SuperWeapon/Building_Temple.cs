using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_Temple : TRBuilding, IRocketSilo
    {
        private int tick = 1000;
        private int maxTick = 1000;
        private int rotation = 45;

        private bool[] bools = new bool[3] { true, true, true };
        private float nukeOffset = 0.55f;
        private float nukeIdle = 0.25f;

        public override Vector3[] DrawPositions => new Vector3[] { base.DrawPos, base.DrawPos, base.DrawPos, base.DrawPos + new Vector3(0, 0, -0.25f) };
        public override Color[] ColorOverrides => new Color[] { Color.white, Color.white };
        public override float[] OpacityFloats => new float[] { 1f, 1f };
        public override float?[] RotationOverrides => new float?[] { null, null };
        public override bool[] DrawBools => new bool[] { bools[0], bools[1], bools[2], true};

        public Vector3 RocketBaseOffset { get; }
        public AltitudeLayer Altitude { get; }

        [TweakValue("NodNukeOffY", 0f, 1f)]
        public static float NodNukeOffY = 0.37f;

        [TweakValue("NodNukePosZ", 0f, 15f)]
        public static float NodNukePosZ = 0f;

        public override Action<FXGraphic>[] Actions => new Action<FXGraphic>[] { null, null, null, delegate (FXGraphic graphic){
            var blades = (Graphic_NumberedCollection)graphic.Graphic;
            for(int i = 0; i < blades.Count; i++)
            {
                Graphic g = blades.Graphics[i];
                var mesh = g.MeshAt(Rotation);
                var drawPos = DrawPositions[3];
                drawPos += Quaternion.Euler(0, i * (180f / blades.Count), 0) * new Vector3(-2, 0, 0) * CurPct;
                drawPos.y = AltitudeLayer.Building.AltitudeFor();
                Graphics.DrawMesh(mesh, drawPos, CurRot.ToQuat(), g.MatSingle, 0);
            }
        }, delegate (FXGraphic graphic){
            Material nukeMat = graphic.Graphic.MatSingle;
            nukeMat.SetTextureOffset("_MainTex", new Vector2(0.25f, NukeOffset + NodNukeOffY));
            nukeMat.SetTextureScale("_MainTex", new Vector2(0.5f, 0.5f));
            Matrix4x4 matrix4x = default(Matrix4x4);
            var pos = new Vector3(DrawPos.x, graphic.altitude, DrawPos.z + 2.55f);
            pos.z += NodNukePosZ;
            matrix4x.SetTRS(pos, Quaternion.Euler(Vector3.up), new Vector3(2f, 1f, 6f));
            Graphics.DrawMesh(MeshPool.plane10, matrix4x, nukeMat, 0);
        }};

        public override bool ShouldDoEffecters => true;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Tick()
        {
            base.Tick();
            if(tick < maxTick)
                tick++;
        }

        public float NukeOffset => Mathf.Lerp(nukeOffset, nukeIdle, CurPct);

        public float CurPct => (float)tick / maxTick;

        public float CurRot => rotation * CurPct;

        public override void Draw()
        {
            base.Draw();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            yield return new Command_Action
            {
                defaultLabel = "start animation",
                action = delegate
                {
                    tick = 0;
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "remove overlays",
                action = delegate
                {
                    bools[0] = !bools[0];
                    bools[1] = !bools[1];
                    bools[2] = !bools[2];
                }
            };

        }
    }
}
