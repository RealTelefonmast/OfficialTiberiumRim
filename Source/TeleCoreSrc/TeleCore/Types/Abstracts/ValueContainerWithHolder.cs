using TeleCore.Defs;
using TeleCore.FlowCore;
using TeleCore.Types.Exposables;
using TeleCore.Types.Structs;

namespace TeleCore.Types.Abstracts;

//Container Template implementing IContainerHolder
public abstract class ValueContainerWithHolder<TValue, THolder> : ValueContainerBase<TValue>
    where TValue : FlowValueDef
    where THolder : IContainerHolderBase<TValue>
{
    protected ValueContainerWithHolder(ContainerConfig<TValue> config, THolder holder) : base(config)
    {
        Holder = holder;
    }

    public THolder Holder { get; }

    public override void Notify_ContainerStateChanged(NotifyContainerChangedArgs<TValue> stateChangeArgs)
    {
        Holder?.Notify_ContainerStateChanged(stateChangeArgs);
    }
}