using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

/* Update Flow
 * Any room-structure changed (wall)
 * Add actions for each change within the tick
 * Dicard previous actions on the same room when the room has not been yet updated
 * Update rooms in next tick based on delayed list
 * => efficiency
 */

public class MapInformation_Rooms : MapInformation
{
    //DEBUG
    private static readonly char check = '✓';
    private static readonly char fail = '❌';

    public MapInformation_Rooms(Map map) : base(map)
    {
        AllTrackers = new Dictionary<Room, RoomTracker>();
        AllList = new List<RoomTracker>();
        TrackerUpdater = new RoomTrackerUpdater(this);

        GlobalUpdateEventHandler.GameUITick += () => TrackerUpdater.Update();
    }

    public RoomTrackerUpdater TrackerUpdater { get; }

    public Dictionary<Room, RoomTracker> AllTrackers { get; }
    public List<RoomTracker> AllList { get; }

    public RoomTracker this[Room room]
    {
        get
        {
            if (room == null)
            {
                TLog.Warning("Room is null, cannot get tracker.");
                VerifyState();
                return null;
            }

            if (!AllTrackers.ContainsKey(room)) return null;
            return AllTrackers[room];
        }
    }

    public RoomTracker this[District district]
    {
        get
        {
            if (district == null || district.Room == null)
            {
                TLog.Warning($"District({district?.ID}) or Room ({district?.Room?.ID}) is null, cannot get tracker.");
                VerifyState();
                return null;
            }

            if (!AllTrackers.ContainsKey(district.Room))
            {
                TLog.Warning($"RoomMapInfo doesn't contain {district.Room.ID}");
                VerifyState();
                return null;
            }

            return AllTrackers[district.Room];
        }
    }

    public override void ExposeDataExtra()
    {
        //Scribe_Collections.Look(ref allTrackers, "trackers", LookMode.Deep);
    }

    public override void ThreadSafeInit()
    {
        foreach (var tracker in AllList) tracker.FinalizeMapInit();
    }

    //
    public override void Tick()
    {
        foreach (var tracker in AllList) tracker.RoomTick();
    }

    //Data Updates
    public void Reset()
    {
        AllTrackers.Clear();
        AllList.Clear();
    }

    public void SetTracker(RoomTracker tracker)
    {
        if (AllTrackers.TryAdd(tracker.Room, tracker))
            AllList.Add(tracker);
        else
            TLog.Warning($"Tried to add tracker with existing key: {tracker.Room.ID} | Outside: {tracker.IsOutside}");
    }

    public void ClearTrackers()
    {
        for (var i = AllList.Count - 1; i >= 0; i--)
        {
            var tracker = AllList[i];
            AllTrackers.Remove(tracker.Room);
            AllList.Remove(tracker);
        }
    }

    public void MarkDisband(RoomTracker tracker)
    {
        tracker.MarkDisbanded();
    }

    public void Disband(RoomTracker tracker)
    {
        tracker.Disband(Map);
        AllTrackers.Remove(tracker.Room);
        AllList.Remove(tracker);
    }

    public void Notify_RoofChanged(Room room)
    {
        if (!AllTrackers.TryGetValue(room, out var tracker)) return;
        tracker?.Notify_RoofChanged();
    }


    public void Notify_RegisterThing(Thing thing, Room room)
    {
        if (room is null) return;
        var tracker = this[room];
        if (tracker is null) return;
        if (!tracker.ListerThings.Contains(thing))
            tracker.Notify_RegisterThing(thing);
    }

    public void Notify_DeregisterThing(Thing thing, Room room)
    {
        if (room is null) return;

        var tracker = this[room];
        if (tracker is null) return;
        if (tracker.ListerThings.Contains(thing))
            tracker.Notify_DeregisterThing(thing);
    }

    public void VerifyState()
    {
        var allRooms = Map.regionGrid.allRooms;
        var trackerCount = AllList.Count;
        var roomCount = allRooms.Count;
        var hitCount = 0;
        var failedTrackers = new List<RoomTracker>();
        foreach (var tracker in AllList)
            if (allRooms.Contains(tracker.Room))
                hitCount++;
            else
                failedTrackers.Add(tracker);

        var ratio = TMath.Round(roomCount / (float)trackerCount, 1);
        var ratioBool = ratio == 1;
        var ratioString =
            $"[{roomCount}/{trackerCount}][{ratio}]{(ratioBool ? check : fail)}".Colorize(ratioBool
                ? Color.green
                : Color.red);

        var hitCountRatio = TMath.Round(hitCount / (float)roomCount, 1);
        var hitBool = hitCountRatio == 1;
        var hitCountRatioString =
            $"[{hitCount}/{roomCount}][{hitCountRatio}]{(hitBool ? check : fail)}".Colorize(
                hitBool ? Color.green : Color.red);
        TLog.Message(
            $"[Verifying RoomMapInfo] Room/Tracker Ratio: {ratioString} | HitCount Test: {hitCountRatioString}");

        if (failedTrackers.Count > 0)
        {
            TLog.Message($"Failed Tracker Count: {failedTrackers.Count}");
            TLog.Message($"Failed Trackers: {failedTrackers.Select(t => t.Room.ID).ToStringSafeEnumerable()}");
        }
    }

    #region Rendering

    public override void UpdateOnGUI()
    {
        if (Find.CurrentMap != Map)
            return;
        foreach (var tracker in AllList)
            tracker.RoomOnGUI();

        if (TeleCoreDebugViewSettings.DrawRoomLabels)
            foreach (var tracker in AllList)
                GenMapUI.DrawThingLabel(GenMapUI.LabelDrawPosFor(tracker.MinVec), $"[{tracker?.Room?.ID}]",
                    Color.white);
    }

    public override void Update()
    {
        if (Find.CurrentMap != Map) return;

        foreach (var tracker in AllList)
            tracker.RoomDraw();

        if (TeleCoreDebugViewSettings.ShowRoomtrackers)
        {
            var intVec = Verse.UI.MouseCell();
            if (intVec.InBounds(map))
            {
                var room = intVec.GetRoom(map);
                if (room != null)
                {
                    var tracker = this[room];
                    if (tracker == null)
                        CellRenderer.RenderCell(intVec, BaseContent.BadMat);
                    else
                        tracker.DrawDebug();
                }
            }
        }
    }

    #endregion
}