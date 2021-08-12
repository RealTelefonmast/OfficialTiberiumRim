using System.Collections;
using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class RoomMapInfo : MapInformation
    {
        public RoomTrackerUpdater updater;
        private Dictionary<Room, RoomTracker> allTrackers = new Dictionary<Room, RoomTracker>();
        private List<RoomTracker> TrackerSet = new List<RoomTracker>();

        public Dictionary<Room, RoomTracker> AllTrackers
        {
            get => allTrackers;
            private set => allTrackers = value;
        }

        public RoomTracker this[Room room] => allTrackers[room];
        public RoomTracker this[District district] => allTrackers[district.Room];

        public RoomMapInfo(Map map) : base(map)
        {
            updater = new RoomTrackerUpdater(this);
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

        public void Notify_RoofChanged(Room room)
        {
            AllTrackers[room].Notify_RoofChanged();
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
