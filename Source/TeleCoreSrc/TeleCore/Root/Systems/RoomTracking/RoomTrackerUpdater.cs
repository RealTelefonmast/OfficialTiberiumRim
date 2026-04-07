using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TeleCore.Systems.RoomTracking
{
    public class RoomTrackerUpdater
    {
        private RoomMapInfo parentInfo;
        private List<RoomTracker> existingTrackers = new List<RoomTracker>();
        private List<RoomTracker> newTrackers = new List<RoomTracker>();

        public List<RoomGroup> reusedRoomGroups = new List<RoomGroup>();
        public List<RoomGroup> newRoomGroups = new List<RoomGroup>();

        public RoomTrackerUpdater(RoomMapInfo mapInfo)
        {
            parentInfo = mapInfo;
        }

        //Notify roof change on this room group
        public void Notify_RoofChanged(RoomGroup group)
        {
            parentInfo.Notify_RoofChanged(group);
        }

        //Initial step of room update - setting known data
        public void Notify_RoomUpdatePrefix()
        {
            existingTrackers = parentInfo.AllTrackers.Values.ToList();
            parentInfo.ClearTrackers();

            reusedRoomGroups.Clear();
            newRoomGroups.Clear();
        }

        //Passing newly generated rooms 
        public void Notify_SetNewRoomData(List<RoomGroup> newRooms, HashSet<RoomGroup> reusedGroup)
        {
            newRoomGroups = newRooms.ListFullCopy();
            reusedRoomGroups = reusedGroup.ToList();
        }

        //Last step, comparing known data, with new generated rooms
        public void Notify_RoomUpdatePostfix()
        {
            //Get all rooms after vanilla updater finishes
            var allRooms = parentInfo.Map.regionGrid.allRooms;

            //Iterate through all rooms
            foreach (var newRoom in allRooms)
            {
                if (Enumerable.Any(newTrackers, t => t.Group == newRoom.Group)) continue;
                //Compare if any known rooms still exist
                var tracker = existingTrackers.Find(t => t.Group == newRoom.Group);
                if (tracker != null)
                {
                    //Notify Tracker Changed
                    if (reusedRoomGroups.Contains(tracker.Group))
                    {
                        tracker.Notify_Reused();
                        tracker.PreApply();
                    }
                    newTrackers.Add(tracker);
                    continue;
                }
                //Compare with new generated rooms
                foreach (var newGroup in newRoomGroups)
                {
                    if (newRoom.Group == newGroup)
                    {
                        var newTracker = new RoomTracker(newGroup);
                        newTrackers.Add(newTracker);
                        newTracker.PreApply();
                        break;
                    }
                }
            }

            //Compare old rooms with new rooms to disband unused ones
            var disbanded = existingTrackers.Except(newTrackers);
            foreach (var tracker in disbanded)
            {
                parentInfo.MarkDisband(tracker);
            }
            foreach (var tracker in disbanded)
            {
                parentInfo.Disband(tracker);
            }

            //Finalize Addition
            foreach (var tracker in newTrackers)
            {
                parentInfo.SetTracker(tracker);
            }

            foreach (var tracker in newTrackers)
            {
                tracker.FinalizeApply();
            }

            newTrackers.Clear();
            existingTrackers = null;
        }
    }
}
