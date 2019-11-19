using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_GrowthSuppressor : FXBuilding
    {
        public int tick = 0; 
        public bool[] bools = new bool[3];
        public override Vector3[] DrawPositions => new Vector3[1] { base.DrawPos };
        public override Color[] ColorOverrides => new Color[1] { Color.white };
        public override float[] OpacityFloats => new float[1] { 1f };
        public override float?[] RotationOverrides => new float?[1] { null };
        public override bool[] DrawBools => new bool[4] { true , bools[0], bools[1], bools[2] };
        public override bool ShouldDoEffecters => true;

        public override void Tick()
        {
            tick++;
            if (tick < 100)
            {
                bools[0] = true;
                return;
            }
            else if (tick < 200)
            {
                bools[1] = true;
                return;
            }
            else if (tick < 300)
            {
                bools[0] = false;
                bools[1] = false;
                return;
            }
            tick = 0;
            base.Tick();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
                yield return g;

            yield return new Command_Action{
                defaultLabel = "center glow",
                action = delegate
                {
                    bools[2] = !bools[2];
                }
            };
        }
    }
}
