using System;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace TR
{
    public class TiberiumTickManager
    {
        private Stopwatch clock = new Stopwatch();

        private float realTimeToTickThrough;
        private bool isPaused = false;

        private Action UITickers;
        private Action GameTickers;

        private int timeControlTicks;

        public bool Paused => isPaused;

        public bool GameActive => Current.Game != null && Current.ProgramState == ProgramState.Playing;
        public bool GamePaused => !GameActive || Find.TickManager.Paused;

        public int CurrentTick => timeControlTicks;

        private float ReusedTickRateMultiplier
        {
            get
            {
                if (!GameActive) return 0;
                return Find.TickManager?.TickRateMultiplier ?? 0;
            }
        }

        private float CurTimePerTick
        {
            get
            {
                if (!GameActive) return 1f / (60f);

                if (ReusedTickRateMultiplier == 0f) return 0f;

                return 1f / (60f * ReusedTickRateMultiplier);
            }
        }

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
                //Ticking
                timeControlTicks++;

                if(!GamePaused)
                    GameTickers?.Invoke();

                UITickers?.Invoke();


                //
                realTimeToTickThrough -= curTimePerTick;
                num++;

                if (Paused || (float)clock.ElapsedMilliseconds > 1000f / 30f)
                {
                    break;
                }
            }
        }

        public void ClearGameTickers()
        {
            GameTickers = null;
        }

        public void TogglePlay()
        {
            isPaused = !isPaused;
        }

        public void RegisterUITickAction(Action action)
        {
            UITickers += action;
        }

        public void RegisterMapTickAction(Action action)
        {
            GameTickers += action;
        }
    }
}
