using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TeleCore.Unsorted;

public class RoomTrackerUpdater
{
    private readonly RoomTracker?[] _trackerGrid;

    private readonly List<DelayedRoomUpdate> delayedActions = new();
    private readonly List<RegionStateChangedArgs> _delayedCacheReset = new();
    private readonly List<RegionStateChangedArgs> _delayedCacheGet = new();
    private readonly MapInformation_Rooms parent;

    private readonly List<Room> tempNewRooms = new();
    private readonly List<Room> tempReusedRooms = new();
    private int lastGameTick;

    public RoomTrackerUpdater(MapInformation_Rooms parent)
    {
        this.parent = parent;
        _trackerGrid = new RoomTracker[this.parent.Map.cellIndices.NumGridCells];
    }

    public bool IsWorking { get; private set; }

    internal void Update(bool isManual = false)
    {
        //Synchronize with game tick
        if ((!IsWorking || Find.TickManager.TicksGame <= lastGameTick) && !isManual) return;

        for (var i = delayedActions.Count - 1; i >= 0; i--)
        {
            var action = delayedActions[i];
            if (action.Room == null || (action.Room.Dereferenced && action.Type != RoomChangeType.Disbanded))
            {
                delayedActions.Remove(action);
            }

            switch (action.Type)
            {
                case RoomChangeType.Created:
                    var previous = action.Room.Cells.Select(c => _trackerGrid[parent.Map.cellIndices.CellToIndex(c)])
                        .Where(t => t != null).Distinct().ToArray();
                    var newTracker = new RoomTracker(action.Room);
                    action.SetTracker(newTracker);
                    action.SetPrevious(previous);
                    parent.SetTracker(newTracker);
                    break;
                case RoomChangeType.Disbanded:
                    parent.MarkDisband(action.Tracker);
                    break;
            }
        }

        //Reused
        foreach (var action in delayedActions)
        {
            if (action.Type == RoomChangeType.Reused)
            {
                action.Tracker.Reset();
                action.Tracker.Notify_Reused();
                GlobalRoomEventHandler.Rooms.OnRoomReused(new RoomChangedArgs(RoomChangeType.Reused, action.Tracker));
            }
        }

        //Disbanded
        foreach (var action in delayedActions)
        {
            if (action.Type == RoomChangeType.Disbanded)
            {
                parent.Disband(action.Tracker);
                GlobalRoomEventHandler.Rooms.OnRoomDisbanded(new RoomChangedArgs(RoomChangeType.Disbanded, action.Tracker));
            }
        }

        foreach (var action in delayedActions)
        {
            if (action.Type == RoomChangeType.Created)
            {
                action.Tracker.Init(action.Previous);
                GlobalRoomEventHandler.Rooms.OnRoomCreated(new RoomChangedArgs(RoomChangeType.Created, action.Tracker));
            }
        }

        foreach (var action in delayedActions)
        {
            if (action.Type == RoomChangeType.Created)
            {
                action.Tracker.PostInit(action.Previous);
            }
        }

        foreach (var action in _delayedCacheGet)
        {
            //Ignore rooms added during delayed update scope
            if (action.Room.Dereferenced) continue;
            GlobalRoomEventHandler.Rooms.OnRegionStateGetRoomUpdate(action);
        }

        foreach (var action in _delayedCacheReset)
        {
            _trackerGrid[CellUtility.Index(action.Cell, parent.Map)] = null;
            GlobalRoomEventHandler.Rooms.OnRegionStateResetRoomUpdate(action);
        }

        IsWorking = false;
        delayedActions.Clear();
        _delayedCacheReset.Clear();
        _delayedCacheGet.Clear();
    }

    //Cache cells around recently spawned and despawned structures
    internal void Notify_CacheDirtyCell(IntVec3 cell, Region region)
    {
        _trackerGrid[parent.Map.cellIndices.CellToIndex(cell)] = parent[region.Room];
        GlobalRoomEventHandler.Rooms.OnRegionStateCachedRoomUpdate(new RegionStateChangedArgs
        {
            Map = parent.Map,
            Cell = cell,
            Region = region
        });
    }

    internal void Notify_ResetDirtyCell(IntVec3 cell)
    {
        _delayedCacheReset.Add(new RegionStateChangedArgs
        {
            Map = parent.Map,
            Cell = cell,
        });
    }

    internal void Notify_GetDirtyCache(Room room)
    {
        _delayedCacheGet.Add(new RegionStateChangedArgs
        {
            Map = parent.Map,
            Room = room,
        });
    }

    internal void Notify_UpdateStarted()
    {
        if (IsWorking)
        {
            for (var i = delayedActions.Count - 1; i >= 0; i--)
            {
                var delayedRoomUpdate = delayedActions[i];
                if (!(delayedRoomUpdate.Room?.Dereferenced ?? true)) continue;
                delayedActions.Remove(delayedRoomUpdate);
            }

            return;
        }

        IsWorking = true;
    }

    internal void Notify_NewRooms(List<Room> newRoomsData, HashSet<Room> reusedRoomsData)
    {
        tempNewRooms.AddRange(newRoomsData);
        tempReusedRooms.AddRange(reusedRoomsData);
    }

    internal void Notify_Finalize()
    {
        //Compare with new generated rooms
        foreach (var newAddedRoom in tempNewRooms)
            delayedActions.Add(new DelayedRoomUpdate(RoomChangeType.Created, newAddedRoom));

        foreach (var tracker in parent.AllTrackers.Values)
        {
            if (tempReusedRooms.Contains(tracker.Room))
                delayedActions.Add(new DelayedRoomUpdate(RoomChangeType.Reused, tracker));

            if (tracker.Room.Dereferenced)
                delayedActions.Add(new DelayedRoomUpdate(RoomChangeType.Disbanded, tracker));
        }

        lastGameTick = Find.TickManager.TicksGame;
        tempNewRooms.Clear();
        tempReusedRooms.Clear();

        //During map generation, update immediately
        if (Current.ProgramState != ProgramState.Playing)
            Update(true);
    }
}