using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

/// <summary>
///     Implements a container for a <see cref="Room" />
/// </summary>
public interface IContainerHolderRoom<TValue> : IContainerHolderBase<TValue> where TValue : FlowValueDef
{
    public Room Room { get; }
    public RoomComponent RoomComponent { get; }
}