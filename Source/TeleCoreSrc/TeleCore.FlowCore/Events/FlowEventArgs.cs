using TeleCore.Shared;

namespace TeleCore.FlowCore.Events;

public struct FlowEventArgs
{
    public DefValue<NetworkValueDef, float> Value { get; private set; }
    
    public FlowEventArgs(DefValue<NetworkValueDef, float> valueChange)
    {
        Value = valueChange;
    }
}