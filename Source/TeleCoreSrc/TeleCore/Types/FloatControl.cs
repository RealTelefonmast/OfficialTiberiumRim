using System.Text;
using TeleCore.Types.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Types;

public class FloatControl
{
    public enum FCState
    {
        Accelerating,
        Decelerating,
        Sustaining,
        Idle
    }

    private const float deltaTime = 0.016666668f;

    private readonly SimpleCurve AccelerationCurve;
    private readonly SimpleCurve DecelerationCurve;

    private readonly float fixedAcc;
    private readonly float fixedTimeInc;
    private readonly float maxValue;
    private readonly SimpleCurve OutputCurve;
    private float curProgress;

    private bool starting, stopping;

    public FloatControl(float maxValue, float secondsToMax, SimpleCurve accCurve = null, SimpleCurve decCurve = null,
        SimpleCurve outCurve = null)
    {
        this.maxValue = maxValue;
        fixedAcc = maxValue / secondsToMax;
        fixedTimeInc = secondsToMax / deltaTime;

        AccelerationCurve = accCurve ?? new SimpleCurve
        {
            new CurvePoint(0, 0),
            new CurvePoint(1, 1)
        };
        DecelerationCurve = decCurve ?? AccelerationCurve;
        OutputCurve = outCurve ?? new SimpleCurve
        {
            new CurvePoint(0, 0),
            new CurvePoint(1, maxValue)
        };
    }

    public bool ReachedPeak => Mathf.Abs(CurPct - 1f) < 0.001953125f;
    public bool StoppedDead => CurPct == 0f;
    public float CurPct => CurValue / maxValue;
    public float CurValue { get; private set; }

    public float OutputValue => OutputCurve?.Evaluate(CurPct) ?? CurValue;

    public float Acceleration
    {
        get
        {
            if (CurState == FCState.Accelerating)
                return AccelerationCurve.Evaluate(curProgress) * fixedAcc;
            if (CurState == FCState.Decelerating)
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
            if (ReachedPeak) return FCState.Sustaining;
            return FCState.Idle;
        }
    }

    public void Tick()
    {
        if (CurState == FCState.Sustaining) return;
        curProgress = Mathf.Clamp01(curProgress + fixedTimeInc);
        CurValue = Mathf.Clamp(CurValue + Acceleration * deltaTime, 0, maxValue);
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

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CurSate: {CurState}");
        sb.AppendLine($"ReachedPeak: {ReachedPeak}");
        sb.AppendLine($"StoppedDead: {StoppedDead}");
        sb.AppendLine($"CurPct: {CurPct}");
        sb.AppendLine($"CurValue: {CurValue}");
        sb.AppendLine($"OutputValue: {OutputValue}");
        sb.AppendLine($"Acceleration: {Acceleration}");
        return sb.ToString();
    }
}