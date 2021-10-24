using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
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
            /*
            //Sustain control
            if (sustainTimeTicks > 0)
            {
                if (CurrentPct <= 1f)
                {
                    Start();
                }
                else if(curSustainTick < sustainTimeTicks)
                {
                    curSustainTick++;
                }
                else
                {
                    Stop();
                }
            }
            */
            //Manual Control
            if (shouldStop)
            {
                if (curIncreaseTick > 0)
                {
                    curIncreaseTick--;
                }
                if(curIncreaseTick == 0)
                    Start();
            }
            if (shouldStart && curIncreaseTick < increaseTimeTicks)
            {
                curIncreaseTick++;
            }
            if (curIncreaseTick == increaseTimeTicks)
                Stop();
        }
    }

    public class Comp_TiberiumNetworkStructure : Comp_NetworkStructure
    {
        public NetworkComponent TiberiumComp => this[TiberiumDefOf.TiberiumNetwork];
        public NetworkContainer Container => TiberiumComp.Container;
        public bool HasConnection => TiberiumComp.HasConnection;

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
            foreach (Gizmo g in Container.GetGizmos())
            {
                yield return g;
            }

            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
        }
    }
}
