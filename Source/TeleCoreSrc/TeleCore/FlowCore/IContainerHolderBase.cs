using TeleCore.Defs;

namespace TeleCore.Unsorted;

//Base holder provides settings for the container
public interface IContainerHolderBase<TValue> where TValue : FlowValueDef
{
    public string ContainerTitle { get; }

    void Notify_ContainerStateChanged(NotifyContainerChangedArgs<TValue> args);
}