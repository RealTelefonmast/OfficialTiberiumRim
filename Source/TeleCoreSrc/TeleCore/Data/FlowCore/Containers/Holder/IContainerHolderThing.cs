using Verse;

namespace TeleCore.FlowCore.Containers.Holder;

/// <summary>
///     Container Implementation extension which allows you to expose a <see cref="Thing" /> reference
/// </summary>
public interface IContainerHolderThing<TValue> : IContainerHolderBase<TValue> where TValue : FlowValueDef
{
    public Thing Thing { get; }
    public bool ShowStorageGizmo { get; }
}