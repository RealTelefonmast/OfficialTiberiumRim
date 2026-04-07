using System.Diagnostics;
using TeleCore.Loader;
using UnityEngine;
using Verse;

namespace TeleCore;

//Tick systems:
//-tickmanager (like RW)
//-tick with params (receives tickrate for data multiplication, instead of ticking multiple times per frame)
//-tick job, ECS based, using Unity Job-System

public class TeleTickManager
{
    private const float _FPSLimiter = 45.4545441f;
    private readonly Stopwatch clock = new();

    private float realTimeToTickThrough;
    private int ticksThisFrame;
    private int ticksThisFrameNormal;

    public TeleTickManager()
    {
        TLog.Message("Starting TeleTickManager!");
    }

    public bool Paused { get; private set; }

    public static bool GameActive => Current.Game != null && Current.ProgramState == ProgramState.Playing;
    public bool GamePaused => !GameActive || Find.TickManager.Paused;

    public int CurrentTick { get; private set; }

    public int CurrentMapTick { get; private set; }

    private float ReusedTickRateMultiplier
    {
        get
        {
            if (!GameActive) return 0;
            return Find.TickManager?.TickRateMultiplier ?? 0;
        }
    }

    private float CurTimePerTick => 0.016666668F;

    public void Update()
    {
        GameTick();

        //
        if (!GameActive) return;
        for (var i = 0; i < Current.Game.maps.Count; i++)
        {
            var map = Current.Game.maps[i].TeleCore();
            MapTick(map);
            UpdateDrawMap(map);
        }
    }

    //TODO: Check why it ticks 4 times
    private void MapTick(MapComponent_TeleCore map)
    {
        ticksThisFrame = 0;
        if (!Paused)
        {
            var curTimePerTick = Find.TickManager.CurTimePerTick;
            if (Mathf.Abs(Time.deltaTime - curTimePerTick) < curTimePerTick * 0.1f)
                realTimeToTickThrough += curTimePerTick;
            else
                realTimeToTickThrough += Time.deltaTime;
            var tickRateMultiplier = Find.TickManager.TickRateMultiplier;
            clock.Reset();
            clock.Start();
            while (realTimeToTickThrough > 0f && ticksThisFrame < tickRateMultiplier * 2f)
            {
                map.TeleMapSingleTick();
                TeleEventHandler.OnEntityTicked();
                realTimeToTickThrough -= curTimePerTick;
                ticksThisFrame++;
                CurrentMapTick++;
                if (Paused || clock.ElapsedMilliseconds > 45.454544f) break;
            }

            if (realTimeToTickThrough > 0f) realTimeToTickThrough = 0f;
        }
    }

    private void GameTick()
    {
        if (Paused) return;

        //
        var curTimePerTick = CurTimePerTick;
        if (Mathf.Abs(Time.deltaTime - curTimePerTick) < curTimePerTick * 0.1f)
            realTimeToTickThrough += curTimePerTick;
        else
            realTimeToTickThrough += Time.deltaTime;

        ticksThisFrameNormal = 0;
        clock.Reset();
        clock.Start();
        while (realTimeToTickThrough > 0f && ticksThisFrameNormal < 2)
        {
            if (!GamePaused)
            {
                GlobalUpdateEventHandler.OnGameTick();
            }

            GlobalUpdateEventHandler.OnUITick();

            realTimeToTickThrough -= curTimePerTick;
            ticksThisFrameNormal++;
            CurrentTick++;

            if (Paused || clock.ElapsedMilliseconds > _FPSLimiter) break;
        }

        realTimeToTickThrough = 0;
    }

    private void UpdateDrawMap(MapComponent_TeleCore map)
    {
        map?.TeleMapUpdate();
    }

    public void TogglePlay()
    {
        Paused = !Paused;
    }
}