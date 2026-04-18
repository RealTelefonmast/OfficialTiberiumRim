using System;
using TeleCore.Defs;

namespace TeleCore.Unsorted;

public struct ContainerArgs
{
    public string Name { get; }
    public int Capactiy { get; }
    public bool StoreEvenly { get; }
}

public enum NotifyContainerChangedAction
{
    AddedValue,
    RemovedValue,
    Filled,
    Emptied
}

public class NotifyContainerChangedArgs<TValue> : EventArgs
    where TValue : FlowValueDef
{
    public NotifyContainerChangedArgs(DefValueStack<TValue> delta, DefValueStack<TValue> final)
    {
        ValueDelta = delta;

        //Resolve Action
        Action = delta > 0 ? NotifyContainerChangedAction.AddedValue : NotifyContainerChangedAction.RemovedValue;
        if (final.Empty && delta < 0)
            Action = NotifyContainerChangedAction.Emptied;
        if ((final.Full ?? false) && delta > 0)
            Action = NotifyContainerChangedAction.Filled;
    }

    public NotifyContainerChangedAction Action { get; }
    public DefValueStack<TValue> ValueDelta { get; }

    public override string ToString()
    {
        return $"Action: {Action}:\n{ValueDelta}";
    }
}

/*public class ClassA<TInterface, TClass>
    where TInterface : IInterfaceA<TInterface, TClass>
    where TClass : ClassA<TInterface, TClass>
{
    public TInterface Interface { get; }
}

public interface IInterfaceA<TInterface, TClass>
    where TInterface : IInterfaceA<TInterface, TClass>
    where TClass : ClassA<TInterface, TClass>
{
    public TClass Class { get; }
}*/