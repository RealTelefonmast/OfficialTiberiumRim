using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TeleCore.RoomComponents;
using TeleCore.Types.Structs;
using TeleCore.Types.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Types;

public class RoomCompNeighborSet : IEnumerable<RoomComponent>
{
    private readonly Dictionary<TwoWayKey<RoomComponent>, RoomComponentLink> _links;
    private readonly List<RoomComponent> _neighbors;

    public RoomCompNeighborSet()
    {
        _neighbors = new List<RoomComponent>();
        _links = new Dictionary<TwoWayKey<RoomComponent>, RoomComponentLink>();
    }

    public IReadOnlyCollection<RoomComponent> Neighbors => _neighbors;
    public IReadOnlyCollection<RoomComponentLink> Links => _links.Values;

    public IEnumerator<RoomComponent> GetEnumerator()
    {
        return _neighbors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public RoomComponentLink LinkFor(TwoWayKey<RoomComponent> key)
    {
        return _links!.TryGetValue(key)!;
    }

    public void Notify_AddNeighbor<T>(T neighbor) where T : RoomComponent
    {
        _neighbors.Add(neighbor);
    }

    public void Notify_AddLink(RoomComponentLink link)
    {
        var key = (link.A, link.B);
        _links.TryAdd(key, link);
    }

    public void Reset()
    {
        _neighbors.Clear();
        _links.Clear();
    }

    public bool Contains(RoomComponent comp)
    {
        return _neighbors.Contains(comp);
    }

    internal void DrawDebug(RoomComponent comp)
    {
        foreach (var portal in Links)
        {
            GenDraw.DrawFieldEdges(portal.Connector.Position.ToSingleItemList(), Color.red);
            GenDraw.DrawFieldEdges(portal.Opposite(comp).Room.Cells.ToList(), Color.green);
        }
    }
}