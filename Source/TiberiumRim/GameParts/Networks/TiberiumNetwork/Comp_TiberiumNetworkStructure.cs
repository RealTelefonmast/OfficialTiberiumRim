using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class FloatAcceleration
    {
        private const float deltaTime = 0.016666668f;

        //MetaData
        private float increment;
        private SimpleCurve valueCurve;

        //Dynamic
        private float progress;

        private bool start, stop;

        private float Increment => start ? increment : stop ? -increment : 0;

        public bool ReachedPeak => Math.Abs(progress - 1.0f) < 0.0125;

        public float CurPct => progress;
        public float CurValue => valueCurve.Evaluate(CurPct);

        public Func<bool> DoAcceleration;
        public Func<bool> DoDeceleration;

        public FloatAcceleration(float tickIncrement, SimpleCurve outputCurve)
        {
            this.increment = tickIncrement;
            this.valueCurve = outputCurve;
        }

        private void SpeedAt()
        {
            //V = a * t

        }

        public void Tick()
        {
            
            if (start)
            {
                progress = Mathf.Clamp(progress + increment, 0, 1.0f);
            }

            if (stop)
            {
                var decrement = -Mathf.Pow(increment, 2);
                progress = progress = Mathf.Clamp(progress + increment, 0, 1.0f);
            }
        }

        public void Start(bool andSustain)
        {
            start = true;
            stop = false;
        }

        public void Stop()
        {
            stop = true;
            start = false;
        }
    }
    public class FloatControl
    {
        private int curIncreaseTick;
        private int curSustainTick;

        private bool shouldStart;
        private bool shouldStop;

        //
        private readonly float maxValue;
        private readonly int increaseTimeTicks;
        private readonly int sustainTimeTicks;
        private SimpleCurve controlCurve;

        public float CurrentPct => curIncreaseTick / (float)increaseTimeTicks;
        public float CurrentValue => controlCurve?.Evaluate(CurrentPct) ?? Mathf.Lerp(0, maxValue, CurrentPct);

        public FloatControl(float increaseTime, float sustainTime, float maxValue, SimpleCurve controlCurve = null)
        {
            increaseTimeTicks = increaseTime.SecondsToTicks();
            sustainTimeTicks = sustainTime.SecondsToTicks();
            this.maxValue = maxValue;
            this.controlCurve = controlCurve;
            Start();
        }

        public void Start()
        {
            shouldStop = false;
            shouldStart = true;
        }

        public void Stop()
        {
            shouldStart = false;
            shouldStop = true;
            curSustainTick = 0;
        }

        public void Tick()
        {
        }
    }


    public enum FCState
    {
        Accelerating,
        Decelerating,
        Sustaining
    }

    public class FCAccelerator
    {
        private bool start, stop;
        private FC accelerator;

        public FC FC => accelerator;

        public FCState AcceleratorState()
        {
            if (start && !accelerator.ReachedPeak)
                return FCState.Accelerating;
            if (stop && !accelerator.StoppedDead)
                return FCState.Decelerating;
            return FCState.Sustaining;
        }

        public FCAccelerator(float maxAcc, float accTime)
        {
            var accVal = maxAcc / accTime;
            accelerator = new FC(() => maxAcc, () => accVal, AcceleratorState);
        }

        public void Tick()
        {
            accelerator.Tick();
        }

        public void Start()
        {
            start = true;
            stop = false;
        }

        public void Stop()
        {
            stop = true;
            start = false;
        }
    }

    public class FC
    {
        private const float deltaTime = 0.016666668f;

        private readonly Func<float> maxValue;
        private readonly Func<float> accVal;
        private readonly Func<FCState> accState;

        private readonly SimpleCurve outputCurve;

        //
        private float curValue;

        //
        public float Acceleration
        {
            get
            {
                var acc = accVal() * deltaTime;

                if (CurState == FCState.Decelerating)
                    acc = -acc;

                return acc;
            }
        }

        public float MaxVal => maxValue();
        public float AccVal => accVal();

        public FCState CurState => accState();
        public bool ReachedPeak => Math.Abs(CurPct - 1f) < 0.001953125f;
        public bool StoppedDead => CurPct == 0f;
        public float CurPct => curValue / maxValue();
        public float CurValue => curValue;
        public float OutputValue => outputCurve?.Evaluate(CurPct) ?? curValue;


        public FC(Func<float> maxValue, Func<float> accelerationPerSecond, Func<FCState> accState, SimpleCurve outputCurve = null)
        {
            this.accVal = accelerationPerSecond;
            this.accState = accState;
            this.maxValue = maxValue;
            this.outputCurve = outputCurve;
        }

        public void Tick()
        {
            if (CurState == FCState.Sustaining) return;
            curValue = Mathf.Clamp(curValue + Acceleration, 0, maxValue());
        }
    }

    public class Comp_TiberiumNetworkStructure : Comp_NetworkStructure
    {
        public NetworkComponent TiberiumComp => this[TiberiumDefOf.TiberiumNetwork];
        public NetworkContainer Container => TiberiumComp.Container;
        public bool HasConnection => TiberiumComp.HasConnection;

        public new CompProperties_TNS Props => (CompProperties_TNS)base.Props;

        //
        public override Color[] ColorOverrides => new Color[2] {Color, Color.white};

        public Color Color
        {
            get
            {
                if (TiberiumComp.Container != null)
                {
                    return TiberiumComp.Container.Color;
                }
                return Color.magenta;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {

            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.ToString());

            if (DebugSettings.godMode)
            {
                sb.AppendLine("Storage Mode: " + Container.AcceptedTypes.ToStringSafeEnumerable());
            }

            return sb.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
        }
    }
    public class CompProperties_TNS : CompProperties_NetworkStructure
    {
        public CompProperties_TNS()
        {
            this.compClass = typeof(Comp_TiberiumNetworkStructure);
        }
    }
}
