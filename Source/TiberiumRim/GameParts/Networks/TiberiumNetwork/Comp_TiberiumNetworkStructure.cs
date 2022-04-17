using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public enum FCState
    {
        Accelerating,
        Decelerating,
        Sustaining,
        Idle
    }

    public class FCSimple
    {
        private const float deltaTime = 0.016666668f;

        private readonly float fixedAcc;
        private readonly float fixedTimeInc;
        private readonly float maxValue;

        private bool starting, stopping;
        private float curProgress = 0;
        private float curValue;

        private SimpleCurve AccelerationCurve;
        private SimpleCurve DecelerationCurve;
        private SimpleCurve OutputCurve;

        public bool ReachedPeak => Math.Abs(CurPct - 1f) < 0.001953125f;
        public bool StoppedDead => CurPct == 0f;
        public float CurPct => curValue / maxValue;
        public float CurValue => curValue;
        public float OutputValue => OutputCurve?.Evaluate(CurPct) ?? curValue;

        public float Acceleration
        {
            get
            {
                if (CurState == FCState.Accelerating)
                    return AccelerationCurve.Evaluate(curProgress) * fixedAcc;
                if(CurState == FCState.Decelerating)
                    return (DecelerationCurve.Evaluate(curProgress) * fixedAcc).Negate();
                return 0;
            }
        }

        public FCState CurState
        {
            get
            {
                if (starting && !ReachedPeak) return FCState.Accelerating;
                if (stopping && !StoppedDead) return FCState.Decelerating;
                return FCState.Sustaining;
            }
        }

        public FCSimple(float maxValue, float secondsToMax, SimpleCurve accCurve = null, SimpleCurve decCurve = null, SimpleCurve outCurve = null)
        {
            this.maxValue = maxValue;
            fixedAcc = maxValue / secondsToMax;
            fixedTimeInc = secondsToMax / deltaTime;

            AccelerationCurve = accCurve ?? new SimpleCurve()
            {
                new(0, 0),
                new(1, 1),
            };
            DecelerationCurve = decCurve ?? AccelerationCurve;
            OutputCurve = outCurve ?? new SimpleCurve()
            {
                new(0, 0),
                new(1, maxValue),
            };
        }

        public void Tick()
        {
            if (CurState == FCState.Sustaining) return;
            curProgress = Mathf.Clamp01(curProgress + fixedTimeInc);
            curValue = Mathf.Clamp(curValue + Acceleration * deltaTime, 0, maxValue);
        }

        public void Start()
        {
            starting = true;
            stopping = false;
        }

        public void Stop()
        {
            starting = false;
            stopping = true;
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
