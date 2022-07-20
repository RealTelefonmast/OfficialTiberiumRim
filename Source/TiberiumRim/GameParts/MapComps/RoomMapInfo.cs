using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using Verse;

namespace TiberiumRim
{
    public class RoomMapInfo : MapInformation
    {
        public RoomTrackerUpdater updater;
        private Dictionary<Room, RoomTracker> allTrackers = new Dictionary<Room, RoomTracker>();
        private List<RoomTracker> TrackerSet = new List<RoomTracker>();

        private RoomTracker[] RoomTrackerGrid;

        public RoomMapInfo(Map map) : base(map)
        {
            updater = new RoomTrackerUpdater(this);
            RoomTrackerGrid = new RoomTracker[map.cellIndices.NumGridCells];
        }

        public Dictionary<Room, RoomTracker> AllTrackers
        {
            get => allTrackers;
            private set => allTrackers = value;
        }

        public RoomTracker this[Room room]
        {
            get
            {
                if (room == null)
                {
                    TRLog.Warning($"Room is null, cannot get tracker.");
                    VerifyState();
                    return null;
                }
                if (!allTrackers.ContainsKey(room))
                {
                    //TRLog.Warning($"RoomMapInfo doesn't contain Room[ID:{room.ID}] on Map[{room.Map}]");
                    //VerifyState();
                    return null;
                }
                return allTrackers[room];
            }
        }

        public RoomTracker this[District district]
        {
            get
            {
                if (district == null || district.Room == null)
                {
                    TRLog.Warning($"District({district?.ID}) or Room ({district?.Room?.ID}) is null, cannot get tracker.");
                    VerifyState();
                    return null;
                }
                if (!allTrackers.ContainsKey(district.Room))
                {
                    TRLog.Warning($"RoomMapInfo doesn't contain {district.Room.ID}");
                    VerifyState();
                    return null;
                }
                return allTrackers[district.Room];
            }
        }

        public void Notify_ApplyAllTrackers(IEnumerator routine)
        {
            Find.CameraDriver.StartCoroutine(routine);
        }

        public void Reset()
        {
            AllTrackers.Clear();
            TrackerSet.Clear();
        }

        public void SetTracker(RoomTracker tracker)
        {
            AllTrackers.Add(tracker.Room, tracker);
            TrackerSet.Add(tracker);
        }

        public void ClearTrackers()
        {
            for (int i = TrackerSet.Count - 1; i >= 0; i--)
            {
                var tracker = TrackerSet[i];
                AllTrackers.Remove(tracker.Room);
                TrackerSet.Remove(tracker);
            }
        }

        public void MarkDisband(RoomTracker tracker)
        {
            tracker.MarkDisbanded();
        }

        public void Disband(RoomTracker tracker)
        {
            tracker.Disband(Map);
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

        public void Notify_RoofChanged(Room room)
        {
            if(!AllTrackers.TryGetValue(room, out var tracker)) return;
            tracker?.Notify_RoofChanged();
        }

        public override void Tick()
        {
            foreach (RoomTracker tracker in TrackerSet)
            {
                tracker.RoomTick();
            }
        }

        private static char check = '✓';
        private static char fail = '❌';

        public void VerifyState()
        {
            var allRooms = Map.regionGrid.allRooms;
            var trackerCount = TrackerSet.Count;
            var roomCount = allRooms.Count;
            var hitCount = 0;
            var failedTrackers = new List<RoomTracker>();
            foreach (var tracker in TrackerSet)
            {
                if (allRooms.Contains(tracker.Room))
                {
                    hitCount++;
                }
                else
                {
                    failedTrackers.Add(tracker);
                }
            }

            var ratio = Math.Round(roomCount / (float)trackerCount, 1);
            var ratioBool = ratio == 1;
            var ratioString = $"[{roomCount}/{trackerCount}][{ratio}]{(ratioBool ? check:fail)}".Colorize(ratioBool ? Color.green : Color.red);

            var hitCountRatio = Math.Round(hitCount / (float)roomCount,1);
            var hitBool = hitCountRatio == 1;
            var hitCountRatioString = $"[{hitCount}/{roomCount}][{hitCountRatio}]{(hitBool ? check : fail)}".Colorize(hitBool ? Color.green : Color.red);
            TRLog.Debug($"[Verifying RoomMapInfo] Room/Tracker Ratio: {ratioString} | HitCount Test: {hitCountRatioString}");

            if (failedTrackers.Count > 0)
            {
                TRLog.Debug($"Failed Tracker Count: {failedTrackers.Count}");
                TRLog.Debug($"Failed Trackers: {failedTrackers.Select(t => t.Room.ID).ToStringSafeEnumerable()}");
            }
        }

        public override void UpdateOnGUI()
        {
            if (Find.CurrentMap != Map) return;
            foreach (RoomTracker tracker in TrackerSet)
            {
                tracker.RoomOnGUI();
            }
        }

        public override void Draw()
        {
            if (Find.CurrentMap != Map) return;
            foreach (RoomTracker tracker in TrackerSet)
            {
                tracker.RoomDraw();
            }
        }
    }
}
