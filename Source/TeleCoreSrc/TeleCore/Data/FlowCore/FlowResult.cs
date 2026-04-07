using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TeleCore.Math;
using TeleCore.Primitive;
using UnityEngine;

namespace TeleCore.FlowCore;

public enum FlowState
{
    Failed,
    Completed,
    CompletedWithExcess,
    CompletedWithShortage,
}

public enum FlowFailureReason
{    
    None,
    TransferOverflow,
    TransferUnderflow,
    TriedToAddToFull,
    TriedToRemoveEmptyValue,
    TriedToConsumeMoreThanExists,
    UsedForbiddenValueDef,
    IllegalState
}

public enum FlowOperation
{
    Add,
    Remove,
    Transfer
}

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

    private FlowResultStack(DefValueStack<TDef,float> desired)
    {
        Desired = desired;
    }
    
    public static FlowResultStack<TDef> Init(DefValueStack<TDef, float> desired, FlowOperation opType)
    {
        if(opType == FlowOperation.Remove)
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

/*OLD REF
namespace TeleCore.Generics.Container;

/*public struct FlowResult
{
    private bool hadFlow = false;
    private bool flowToOther = false;
    private bool isVoided = false;
    private ValueFlowDirection flowDirection = ValueFlowDirection.None;

    public bool NoFlow => !hadFlow;
    public bool FlowsToOther => flowToOther;
    public bool IsVoided => isVoided;

    public int FromIndex
    {
        get
        {
            return flowDirection switch
            {
                ValueFlowDirection.Positive => 0,
                ValueFlowDirection.Negative => 1,
                _ => -1
            };
        }
    }

    public int ToIndex
    {
        get
        {
            return flowDirection switch
            {
                ValueFlowDirection.Positive => 1,
                ValueFlowDirection.Negative => 0,
                _ => -1
            };
        }
    }

    public FlowResult() { }

    public FlowResult(ValueFlowDirection flowDir)
    {
        hadFlow = flowToOther = true;
        flowDirection = flowDir;
    }

    public void SetFlow(ValueFlowDirection flowDir)
    {
        hadFlow = flowToOther = true;
        this.flowDirection = flowDir;
    }

    public static FlowResult None => new() {hadFlow = false};
    public static FlowResult ResultVoided => new() {isVoided = true, hadFlow = true };
    public static FlowResult ResultNormalFlow => new() {flowToOther = true, hadFlow = true};

    public override string ToString()
    {
        return $"HadFlow: {hadFlow}; FlowToOther: {flowToOther}; IsVoided: {isVoided}; FlowDir: {flowDirection} [{FromIndex} -> {ToIndex}]";
    }
}* /
*/