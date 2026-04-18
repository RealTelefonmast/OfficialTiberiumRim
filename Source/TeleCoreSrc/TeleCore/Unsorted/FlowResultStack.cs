using TeleCore.Defs;

namespace TeleCore.Unsorted;

public struct FlowResultStack<TDef>
    where TDef : FlowValueDef
{
    public DefValueStack<TDef, float> Desired { get; }
    public DefValueStack<TDef, float> Actual { get; private set; }
    public FlowFailureReason Reason { get; private set; }

    public DefValueStack<TDef, float> Diff => Desired - Actual;
    private float DiffValue => Desired.TotalValue - Actual.TotalValue;

    public FlowState State
    {
        get
        {
            if (Reason != FlowFailureReason.None)
                return FlowState.Failed;

            if (DiffValue <= float.Epsilon)
                return FlowState.Completed;
            if (DiffValue > 0)
                return FlowState.CompletedWithExcess;
            if (DiffValue < 0)
                return FlowState.CompletedWithShortage;

            return FlowState.Failed;
        }
    }

    public static implicit operator bool(FlowResultStack<TDef> result) => result.State != FlowState.Failed;

    private FlowResultStack(DefValueStack<TDef, float> desired)
    {
        Desired = desired;
    }

    public static FlowResultStack<TDef> Init(DefValueStack<TDef, float> desired, FlowOperation opType)
    {
        if (opType == FlowOperation.Remove)
            desired *= -1;
        return new FlowResultStack<TDef>(desired);
    }

    public FlowResultStack<TDef> AddResult(DefValue<TDef, float> result)
    {
        Actual += result;
        return this;
    }

    public FlowResultStack<TDef> AddResult(FlowResult<TDef, float> subResult)
    {
        Actual += (subResult.Def, subResult.Actual);
        return this;
    }

    public FlowResultStack<TDef> Fail(FlowFailureReason reason)
    {
        Reason = reason;
        return this;
    }
}