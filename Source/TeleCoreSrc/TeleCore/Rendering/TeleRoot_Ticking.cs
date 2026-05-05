using System.Diagnostics;
using TeleCore.Types.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Rendering;

public class TeleRoot_Ticking : MonoBehaviour
{
    private readonly float _tickDelta = 0.016666668F; // 1/60
    private readonly long _tickDeltaMilliseconds = 16;

    private readonly Stopwatch _watch = new();
    private float deltaTimeCounter;

    private bool Paused => Find.TickManager.Paused;
    private float TickRate => Find.TickManager.TickRateMultiplier;

    private void Update()
    {
        if (Paused) return;
        _watch.Reset();
        _watch.Start();

        deltaTimeCounter += Time.deltaTime;
        if (deltaTimeCounter >= _tickDelta)
        {
            var time = _watch.ElapsedMilliseconds;
            if (time > _tickDeltaMilliseconds || time < _tickDeltaMilliseconds)
                TLog.Message($"Tick was outside of expected duration, took {time}ms");
            deltaTimeCounter -= _tickDelta;
            //TeleEventHandler.OnTick(TickRate);
        }
    }
}