using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TeleCore.Unsorted;

//TODO: Implement Workers and replace AtmosphericPortal in TAC
public abstract class RoomPortalWorker
{
    protected RoomTrackerLink parent;

    public RoomPortalWorker(RoomTrackerLink parent)
    {
        this.parent = parent;
    }
}

public class RoomTrackerLink
{
    private readonly Rot4[] _connectionDirections;
    private readonly RoomTracker[] _connections;
    private readonly RoomTracker _room;
    private readonly Building _connector;

    private List<RoomPortalWorker> _workers;

    public RoomTrackerLink(Building connector, RoomTracker portalRoom)
    {
        _connector = connector;
        _room = portalRoom;
    }

    public RoomTrackerLink(Building connector, RoomTracker roomA, RoomTracker roomB, RoomTracker portalRoom)
    {
        _connector = connector;
        _room = portalRoom;
        _connections = new[] { roomA, roomB };
        _connectionDirections = new Rot4[2];

        //Get Directions
        for (var i = 0; i < 4; i++)
        {
            var cell = connector.Position + GenAdj.CardinalDirections[i];
            var room = cell.GetRoomFast(connector.Map);
            if (room == null) continue;
            if (roomA.Room == room)
                _connectionDirections[0] = cell.Rot4Relative(connector.Position);
            if (roomB.Room == room)
                _connectionDirections[1] = cell.Rot4Relative(connector.Position);
        }

        //Generate Workers
        var subclasses = typeof(RoomPortalWorker).AllSubclassesNonAbstract();
        if (subclasses is not { Count: > 0 }) return; // Early exit if "subclasses" is null or empty.
        _workers = subclasses.Select(type => (RoomPortalWorker)Activator.CreateInstance(type, this)).ToList();
    }

    public bool ConnectsToOutside => _connections[0].IsOutside || _connections[1].IsOutside;

    public bool ConnectsToSame =>
        (_connections[0].IsOutside && _connections[1].IsOutside) || _connections[0] == _connections[1];

    public bool IsValid => _connector != null;

    public Thing Connector => _connector;
    public RoomTracker PortalRoom => _room;

    public RoomTracker this[int i] => _connections[i];

    //Helpers
    public int IndexOf(RoomTracker tracker)
    {
        return _connections[0] == tracker ? 0 : 1;
    }

    public RoomTracker Opposite(RoomTracker other)
    {
        return other == _connections[0] ? _connections[1] : _connections[0];
    }

    public bool Connects(RoomTracker toThis)
    {
        return toThis == _connections[0] || toThis == _connections[1];
    }

    //
    public override string ToString()
    {
        return $"{_connections[0].Room.ID} -[{Connector}]-> {_connections[1].Room.ID}";
    }
}