using TeleCore.Defs;
using TeleCore.RoomComponents;
using Verse;

namespace TeleCore.FlowCore;

/// <summary>
///     Implements a container for a <see cref="Room" />
/// </summary>
public interface IContainerHolderRoom<TValue> : IContainerHolderBase<TValue> where TValue : FlowValueDef
{
    public Room Room { get; }
    public RoomComponent RoomComponent { get; }
}