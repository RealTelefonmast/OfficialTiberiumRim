using System;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace TR.GameParts.GameUpdate;

public class TiberiumTickManager
{
    private readonly Stopwatch clock = new();
    private Action GameTickers;

    private float realTimeToTickThrough;

    private Action UITickers;

    public bool Paused { get; private set; }

    public bool GameActive => Current.Game != null && Current.ProgramState == ProgramState.Playing;
    public bool GamePaused => !GameActive || Find.TickManager.Paused;

    public int CurrentTick { get; private set; }

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
            if (!GameActive) return 1f / 60f;

            if (ReusedTickRateMultiplier == 0f) return 0f;

            return 1f / (60f * ReusedTickRateMultiplier);
        }
    }

    public void Update()
    {
        if (Paused) return;
        var curTimePerTick = CurTimePerTick;
        if (Mathf.Abs(Time.deltaTime - curTimePerTick) < curTimePerTick * 0.1f)
            realTimeToTickThrough += curTimePerTick;
        else
            realTimeToTickThrough += Time.deltaTime;

        var num = 0;
        clock.Reset();
        clock.Start();
        while (realTimeToTickThrough > 0f && (float)num < 2)
        {
            //Ticking
            CurrentTick++;

            if (!GamePaused)
                GameTickers?.Invoke();

            UITickers?.Invoke();


            //
            realTimeToTickThrough -= curTimePerTick;
            num++;

            if (Paused || clock.ElapsedMilliseconds > 1000f / 30f) break;
        }
    }

    public void ClearGameTickers()
    {
        GameTickers = null;
    }

    public void TogglePlay()
    {
        Paused = !Paused;
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