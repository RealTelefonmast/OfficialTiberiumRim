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
        private float speedInt = 0;
        private int currentSpeedUpTick = -1;
        private int currentIdleTicks = -1;

        private const int speedUpTicks = 500;
        private const int idleTime = 1000;

        public override float?[] AnimationSpeeds => new float?[5] {null, null, CurrentSpeed, CurrentSpeed, null};

        private float CurrentSpeed => speedInt;
        private bool SpeedingUp => false;

        public SimpleCurve Curve = new SimpleCurve()
        {
            new CurvePoint(0, 0),
            new CurvePoint(0.5f, 3),
            new CurvePoint(0.8f, 6),
            new CurvePoint(1, 10),
        };
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Log.Message("Setting up centrifuge...");
        }

        public override void CompTick()
        {
            base.CompTick();
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
        }
    }
}
