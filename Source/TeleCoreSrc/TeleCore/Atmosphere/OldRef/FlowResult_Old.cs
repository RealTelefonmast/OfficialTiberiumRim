// Preserved from TeleCore/RoomComponents/FlowResult.cs (commented-out struct, not atmosphere-specific)

namespace TeleCore.Atmosphere.OldRef;
/*
public struct FlowResult
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
            flowres
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
}
*/