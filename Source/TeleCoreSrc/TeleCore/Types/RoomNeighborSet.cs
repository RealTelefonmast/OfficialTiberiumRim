using System.Collections.Generic;

namespace TeleCore.Unsorted;

/// <summary>
///     A room's neighbor can be defined in different ways:
///     <para>- A true neighbor (ie: Doorways)</para>
///     <para>- Any attached room (Rooms on the other side of doors/walls)</para>
///     So we need to track of that.
/// </summary>
public class RoomNeighborSet
{
    private readonly List<RoomTracker> _attachedNghb;
    private readonly List<RoomTracker> _trueNghb;

    public RoomNeighborSet()
    {
        _trueNghb = new List<RoomTracker>();
        _attachedNghb = new List<RoomTracker>();
    }

    public IReadOnlyCollection<RoomTracker> TrueNeighbors => _trueNghb;
    public IReadOnlyCollection<RoomTracker> AttachedNeighbors => _attachedNghb;

    public void Notify_AddNeighbor(RoomTracker neighbor)
    {
        _trueNghb.Add(neighbor);
    }

    public void Notify_AddAttachedNeighbor(RoomTracker neighbor)
    {
        if (_attachedNghb.Contains(neighbor)) return;
        _attachedNghb.Add(neighbor);
    }

    public void Reset()
    {
        _trueNghb.Clear();
        _attachedNghb.Clear();
    }
}