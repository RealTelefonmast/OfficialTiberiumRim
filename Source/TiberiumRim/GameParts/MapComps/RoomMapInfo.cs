using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                    Log.Warning($"Room is null, cannot get tracker.");
                    return null;
                }
                if (!allTrackers.ContainsKey(room))
                {
                    Log.Warning($"RoomMapInfo doesn't contain {room.ID}");
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
                    Log.Warning($"District({district?.ID}) or Room ({district?.Room?.ID}) is null, cannot get tracker.");
                    return null;
                }
                if (!allTrackers.ContainsKey(district.Room))
                {
                    Log.Warning($"RoomMapInfo doesn't contain {district.Room.ID}");
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

        public void Notify_ThingSpawned(Thing thing)
        {
            if (!this.map.regionAndRoomUpdater.Enabled && this.map.regionAndRoomUpdater.AnythingToRebuild) return;
            var room = thing.GetRoom();
            if (room == null || !AllTrackers.TryGetValue(room, out var tracker)) return;
            tracker?.Notify_ThingSpawned(thing);
        }

        public void Notify_ThingDespawned(Thing thing)
        {
            if (!this.map.regionAndRoomUpdater.Enabled && this.map.regionAndRoomUpdater.AnythingToRebuild) return;
            var room = thing.GetRoom();
            if (room == null || !AllTrackers.TryGetValue(room, out var tracker)) return;
            tracker?.Notify_ThingDespawned(thing);
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

        public override void UpdateOnGUI()
        {
            foreach (RoomTracker tracker in TrackerSet)
            {
                tracker.RoomOnGUI();
            }
        }

        public override void Draw()
        {
            foreach (RoomTracker tracker in TrackerSet)
            {
                tracker.RoomDraw();
            }
        }
    }
}
