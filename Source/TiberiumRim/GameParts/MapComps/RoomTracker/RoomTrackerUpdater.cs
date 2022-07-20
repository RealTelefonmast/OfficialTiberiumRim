using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Verse;

namespace TiberiumRim
{
    public class RoomTrackerUpdater
    {
        //private Stopwatch sw = new Stopwatch();

        private RoomMapInfo parentInfo;
        private List<RoomTracker> existingTrackers = new();

        private readonly List<RoomTracker> newTrackers = new();
        private readonly List<RoomTracker> newExistingTrackers = new();
        private readonly List<RoomTracker> reusedTrackers = new();

        public List<Room> reusedOldRooms = new();
        public List<Room> newRooms = new();

        public RoomTrackerUpdater(RoomMapInfo mapInfo)
        {
            parentInfo = mapInfo;
        }

        //Notify roof change on this room group
        public void Notify_RoofChanged(Room room)
        {
            parentInfo.Notify_RoofChanged(room);
        }

        //Initial step of room update - setting known data
        public void Notify_RoomUpdatePrefix()
        {
            existingTrackers = parentInfo.AllTrackers.Values.ToList();
            parentInfo.ClearTrackers();

            reusedOldRooms.Clear();
            newRooms.Clear();
        }

        //Passing newly generated rooms 
        public void Notify_SetNewRoomData(List<Room> newRooms, HashSet<Room> reusedRooms)
        {
            this.newRooms = newRooms.ListFullCopy();
            reusedOldRooms = reusedRooms.ToList();
        }

        //Last step, comparing known data, with new generated rooms
        public void Notify_RoomUpdatePostfix()
        {
            //Get all rooms after vanilla updater finishes
            var allRooms = parentInfo.Map.regionGrid.allRooms;

            //Iterate through all rooms
            foreach (var newRoom in allRooms)
            {
                if (Enumerable.Any(newTrackers, t => t.Room == newRoom)) continue;
                //Compare if any known rooms still exist
                var tracker = existingTrackers.Find(t => t.Room == newRoom);
                if (tracker != null)
                {
                    //Notify Tracker Changed
                    if (reusedOldRooms.Contains(tracker.Room))
                    {
                        reusedTrackers.Add(tracker);
                    }
                    newExistingTrackers.Add(tracker);
                    continue;
                }

                //Compare with new generated rooms
                foreach (var newAddedRoom in newRooms)
                {
                    if (newRoom == newAddedRoom)
                    {
                        var newTracker = new RoomTracker(newAddedRoom);
                        newTrackers.Add(newTracker);
                        break;
                    }
                }
            }

            //Compare old rooms with new rooms to disband unused ones
            var allActiveTrackers = newTrackers.Concat(newExistingTrackers).ToList();
            var disbanded = existingTrackers.Except(allActiveTrackers).ToList();
            foreach (var tracker in disbanded)
            {
                parentInfo.MarkDisband(tracker);
            }

            //Finalize Addition
            foreach (var tracker in allActiveTrackers)
            {
                parentInfo.SetTracker(tracker);
            }

            //
            foreach (var tracker in reusedTrackers)
            {
                tracker.Notify_Reused();
            }

            //
            foreach (var tracker in disbanded)
            {
                parentInfo.Disband(tracker);
            }

            foreach (var tracker in newTrackers)
            {
                tracker.PreApply();
            }

            foreach (var tracker in newTrackers)
            {
                tracker.FinalizeApply();
            }

            newTrackers.Clear();
            reusedTrackers.Clear();
            newExistingTrackers.Clear();
            existingTrackers = null;
        }
    }
}
