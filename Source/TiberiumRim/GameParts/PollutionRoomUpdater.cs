namespace TiberiumRim
{
    /*
    public class PollutionRoomUpdater
    {
        private AtmosphericMapInfo parentInfo;
        private List<PollutionTracker> existingTrackers = new List<PollutionTracker>();
        private List<PollutionTracker> newTrackers = new List<PollutionTracker>();

        public List<RoomGroup> reusedRoomGroups = new List<RoomGroup>();
        public List<RoomGroup> newRoomGroups = new List<RoomGroup>();

        public PollutionRoomUpdater(AtmosphericMapInfo mapInfo)
        {
            parentInfo = mapInfo;
        }

        public void Notify_RoofChanged(RoomGroup group)
        {
            PollutionTracker tracker = parentInfo.PollutionFor(group);
            if (tracker == null) return;
            if (tracker.OpenRoofCount > 0) return;
            int val = Mathf.RoundToInt(tracker.Capacity * parentInfo.OutsideSaturation);
            tracker.Atmospheric += val;
            parentInfo.OutsidePollution -= val;
        }

        public void Notify_UpdateStart()
        {
            existingTrackers = parentInfo.PollutionTrackers.Values.ToList();
            parentInfo.PollutionTrackers.Clear();

            reusedRoomGroups.Clear();
            newRoomGroups.Clear();
        }

        public void Notify_UpdateRoomGroups(List<RoomGroup> newRooms, HashSet<RoomGroup> reusedGroup)
        {
            newRoomGroups = newRooms.ListFullCopy();
            reusedRoomGroups = reusedGroup.ToList();
            Log.Message("NewGroups: " + newRooms.Count + " ReusedGroups: " + reusedGroup.Count);
        }

        public void Apply(List<Room> newRooms)
        {
            foreach (var newRoom in newRooms)
            {
                if (Enumerable.Any(newTrackers, t => t.Group == newRoom.Group)) continue;
                var tracker = existingTrackers.Find(t => t.Group == newRoom.Group);
                if (tracker != null)
                {
                    if (reusedRoomGroups.Contains(tracker.Group))
                    {
                        Log.Message("Updating reused room with existing tracker");
                        tracker.MarkDirty();
                    }
                    newTrackers.Add(tracker);
                    continue;
                }

                foreach (var reusedRoom in reusedRoomGroups)
                {
                    if (newRoom.Group == reusedRoom)
                    {
                        var tracker2 = existingTrackers.Find(t => t.Group == reusedRoom);
                        Log.Message("Updating reused room with existing tracker " + (tracker2 != null));
                        newTrackers.Add(tracker2);
                        break;
                    }
                }

                foreach (var newGroup in newRoomGroups)
                {
                    if (newRoom.Group == newGroup)
                    {
                        var newTracker = new PollutionTracker(newGroup.Map, newGroup, 0);
                        newTracker.MarkDirty();
                        newTrackers.Add(newTracker);
                        if (newTracker.Group.UsesOutdoorTemperature) break;
                        if (parentInfo.pollutionCache.TryGetAverageRoomPollution(newGroup, out int pollution))
                        {
                            newTracker.Atmospheric = pollution;
                        }
                        break;
                    }
                }
            }

            foreach (var pollutionTracker in newTrackers)
            {
                parentInfo.PollutionTrackers.Add(pollutionTracker.Group, pollutionTracker);
            }

            foreach (var tracker in newTrackers)
            {
                if (tracker.IsDirty)
                    tracker.RegenerateData();
                if (tracker.UsesOutDoorPollution)
                    parentInfo.RecalcOutsideCells();
            }
            newTrackers.Clear();
        }
    }
    */
}
