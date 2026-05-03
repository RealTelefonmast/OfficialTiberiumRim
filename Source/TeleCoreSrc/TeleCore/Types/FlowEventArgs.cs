using System;
using TeleCore.Defs;

namespace TeleCore.Unsorted;

public class FlowEventArgs : EventArgs
{
    public FlowEventArgs(DefValue<NetworkValueDef, double> valueChange)
    {
        Value = valueChange;
    }

    public DefValue<NetworkValueDef, double> Value { get; private set; }
}