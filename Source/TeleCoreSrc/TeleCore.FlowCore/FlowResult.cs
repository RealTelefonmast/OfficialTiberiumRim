using System.Diagnostics;
using TeleCore.Math;

namespace TeleCore.FlowCore;

[DebuggerDisplay("{State}: '{Reason}' | [{Def}]{Actual}/{Actual}")]
public readonly struct FlowResult<TDef, TValue>
    where TDef : FlowValueDef
    where TValue : unmanaged
{
    public TDef Def { get; }
    public Numeric<TValue> Desired { get; }
    public Numeric<TValue> Actual { get; }
    public Numeric<TValue> Diff => Desired - Actual;

    public FlowFailureReason Reason { get; }

    public static implicit operator bool(FlowResult<TDef, TValue> result) => result.State != FlowState.Failed;

    public FlowState State
    {
        get
        {
            if (Reason != FlowFailureReason.None)
                return FlowState.Failed;

            if (Diff <= Numeric<TValue>.Epsilon)
                return FlowState.Completed;
            if (Diff > Numeric<TValue>.Zero)
                return FlowState.CompletedWithExcess;
            if (Diff < Numeric<TValue>.Zero)
                return FlowState.CompletedWithShortage;

            return FlowState.Failed;
        }
    }

    private FlowResult(TDef def, TValue desired, FlowFailureReason reason)
    {
        Def = def;
        Desired = desired;
        Actual = Numeric<TValue>.Zero;
        Reason = reason;
    }

    public FlowResult(TDef def, TValue desired, TValue actual)
    {
        Def = def;
        Desired = desired;
        Actual = actual;
    }

    public static FlowResult<TDef, TValue> InitFailed(TDef def, TValue desired, FlowFailureReason reason)
    {
        //Default constructor is NaN failure.
        return new FlowResult<TDef, TValue>(def, desired, reason);
    }
}