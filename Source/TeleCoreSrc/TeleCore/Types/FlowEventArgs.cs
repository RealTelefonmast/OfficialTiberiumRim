using System;
using TeleCore.Defs;
using TeleCore.Types.Structs;

namespace TeleCore.Types;

public class FlowEventArgs : EventArgs
{
    public FlowEventArgs(DefValue<NetworkValueDef, double> valueChange)
    {
        Value = valueChange;
    }

    public DefValue<NetworkValueDef, double> Value { get; private set; }
}