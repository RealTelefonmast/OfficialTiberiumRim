using TeleCore.FlowCore;

namespace TeleCore.Systems.Events;

public struct VolumeChangedEventArgs<TValue>
where TValue : FlowValueDef
{
    public ChangedAction Action { get; }
    public FlowVolumeBase<TValue> Volume { get; }

    public enum ChangedAction
    {
        Invalid,
        AddedValue,
        RemovedValue,
        Filled,
        Emptied
    }

    public VolumeChangedEventArgs(ChangedAction action, FlowVolumeBase<TValue> volume)
    {
        Action = action;
        Volume = volume;
    }
}