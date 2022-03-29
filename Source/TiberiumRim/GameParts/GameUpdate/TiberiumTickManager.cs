using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TiberiumRim
{
    public class TiberiumTickManager
    {
        private Stopwatch clock = new Stopwatch();

        private float realTimeToTickThrough;
        private bool isPaused = false;
        private Action actions;

        private int timeControlTicks;

        private float CurTimePerTick => 1f / (60f);
        public bool Paused => isPaused;
        public int CurrentTick => timeControlTicks;

        public TiberiumTickManager()
        {
        }

        public void Update()
        {
            if (Paused) return;
            float curTimePerTick = CurTimePerTick;
            if (Mathf.Abs(Time.deltaTime - curTimePerTick) < curTimePerTick * 0.1f)
            {
                realTimeToTickThrough += curTimePerTick;
            }
            else
            {
                realTimeToTickThrough += Time.deltaTime;
            }

            int num = 0;
            clock.Reset();
            clock.Start();
            while (realTimeToTickThrough > 0f && (float)num < 2)
            {
                DoSingleTick();
                realTimeToTickThrough -= curTimePerTick;
                num++;

                if (Paused || (float)clock.ElapsedMilliseconds > 1000f / 30f)
                {
                    break;
                }
            }
        }

        private void DoSingleTick()
        {
            timeControlTicks++;
            actions?.Invoke();
        }

        public void TogglePlay()
        {
            isPaused = !isPaused;
        }

        public void RegisterTickAction(Action action)
        {
            actions += action;
        }
    }
}
