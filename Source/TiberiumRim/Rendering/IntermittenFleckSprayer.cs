using System;
using Verse;

namespace TiberiumRim
{
    public class IntermittenFleckSprayer
    {
        private const int MinTicksBetweenSprays = 500;
        private const int MaxTicksBetweenSprays = 2000;
        private const int MinSprayDuration = 200;
        private const int MaxSprayDuration = 500;
        private const float SprayThickness = 0.6f;

        private Thing parent;
        private Action throwAction;
        private Action startSprayCallback;
        private Action endSprayCallback;
        private int ticksUntilSpray = 500;
        private int sprayTicksLeft;


        public IntermittenFleckSprayer(Thing parent, Action throwAction, Action startCallback = null, Action endCallback = null)
        {
            this.parent = parent;
            this.throwAction = throwAction;
            this.startSprayCallback = startCallback;
            endSprayCallback = endCallback;
        }

        public void SprayerTick()
        {
            if (sprayTicksLeft > 0)
            {
                sprayTicksLeft--;
                if (Rand.Value < 0.6f)
                    throwAction.Invoke();
                if (sprayTicksLeft <= 0)
                {
                    if (endSprayCallback != null) endSprayCallback();
                    ticksUntilSpray = Rand.RangeInclusive(500, 2000);
                }
            }
            else
            {
                ticksUntilSpray--;
                if (ticksUntilSpray <= 0)
                {
                    if (startSprayCallback != null) startSprayCallback();
                    sprayTicksLeft = Rand.RangeInclusive(200, 500);
                }
            }
        }
    }
}
