using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class CompTNS_Centrifuge : Comp_TiberiumNetworkStructure
    {
        private FloatControl speedControl;
        private SimpleCurve shaderCurve;

        public override float?[] AnimationSpeeds => new float?[5] {null, null, speedControl.CurrentValue, speedControl.CurrentValue, null};

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            SimpleCurve Curve = new SimpleCurve()
            {
                new (0, 0),
                new (0.5f, 3),
                new (0.8f, 6),
                new (1, 10),
            };
            shaderCurve = new SimpleCurve()
            {
                new(0, 0),
                new(0.5f, 0),
                new(1,1),
            }; 
            speedControl = new FloatControl(5f, 10, 1, Curve);
        }

        public override void CompTick()
        {
            base.CompTick();
            speedControl.Tick();
            fxComp.Graphics[2].Graphic.MatSingle.SetFloat("_BlendValue", shaderCurve.Evaluate(speedControl.CurrentPct));
            /*
            if (currentSpeedUpTick > speedUpTicks || currentIdleTicks > 0)
            {
                if (currentIdleTicks < idleTime)
                {
                    currentIdleTicks++;
                }
                else
                {
                    currentSpeedUpTick--;
                    if (currentSpeedUpTick == 0)
                        currentIdleTicks = 0;
                }
            }
            else
                currentSpeedUpTick++;

            speedInt = Curve.Evaluate((float)currentSpeedUpTick / speedUpTicks);
            */
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder(base.CompInspectStringExtra());
            sb.AppendLine();
            sb.AppendLine($"Current SpeedUp Pct: {speedControl.CurrentPct}");
            sb.AppendLine($"Current Speed: {speedControl.CurrentValue}");
            sb.Append($"Shader BlendValue: {shaderCurve.Evaluate(speedControl.CurrentPct)}");
            return sb.ToString().TrimEndNewlines();
        }
    }
}
