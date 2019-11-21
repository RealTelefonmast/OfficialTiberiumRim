using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class Building_Obelisk : Building_TRTurret
    {
        private float chargeAmount = 0;

        public float ObeliskCharge
        {
            get
            {
                float ticks = MainGun.props.turretBurstWarmupTime.SecondsToTicks();
                return Mathf.InverseLerp(0, ticks, chargeAmount);
            }
        }

        public override LocalTargetInfo CurrentTarget => MainGun.CurrentTarget;

        public override void Tick()
        {
            base.Tick();
            if (MainGun.burstWarmupTicksLeft > 0 && CurrentTarget.IsValid && !CurrentTarget.ThingDestroyed && chargeAmount < MainGun.props.turretBurstWarmupTime.SecondsToTicks())
            {
                chargeAmount++;
            }
            else if(chargeAmount > 0)
            {
                chargeAmount -= Mathf.Clamp(2 * ObeliskCharge, 0, chargeAmount);
            }
        }

        public override ExtendedGraphicData ExtraData => (def as FXThingDef).extraData;

        public override Vector3[] DrawPositions => new Vector3[] { base.DrawPos, base.DrawPos };
        public override Color[] ColorOverrides => new Color[] { Color.white, Color.white };
        public override float[] OpacityFloats => new float[] { 1f, ObeliskCharge };
        public override float?[] RotationOverrides => new float?[] { null , null};
        public override bool[] DrawBools => new bool[] { true, chargeAmount > 0 };
        public override bool ShouldDoEffecters => true;

        public override void Draw()
        {
            base.Draw();
            if (CurrentTarget.IsValid && CurrentTarget.Thing is Pawn p)
            {
                DrawMarkedForDeath(p);
            }
        }

        private void DrawMarkedForDeath(Pawn target)
        {
            Material mat = MaterialPool.MatFrom(TRMats.MarkedForDeath, ShaderDatabase.MetaOverlay, Color.white);
            float num = (Time.realtimeSinceStartup + 397f * (float)(target.thingIDNumber % 571)) * 4f;
            float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
            num2 = 0.3f + num2 * 0.7f;
            Material material = FadedMaterialPool.FadedVersionOf(mat, num2);
            var c = target.TrueCenter() + new Vector3(0, 0, 1.15f);
            Graphics.DrawMesh(MeshPool.plane08, new Vector3(c.x, AltitudeLayer.MetaOverlays.AltitudeFor(), c.z), Quaternion.identity, material, 0);
        }
    }
}
