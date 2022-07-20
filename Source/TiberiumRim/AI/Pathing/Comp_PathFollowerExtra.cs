using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using TiberiumRim.Utilities;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class Comp_PathFollowerExtra : ThingComp
    {
        private bool lastSetupDirty = false;

        private Room CurrentRoom { get; set; }
        private RoomTracker CurrentTracker { get; set; }
        public RoomComponent_AirLock CurrentAirlock { get; private set; }

        private PawnPath CurrentPath { get; set; }

        private Room LastRoom { get; set; }
        private Room NextRoom { get; set; }

        private RoomTracker LastTracker { get; set; }
        private RoomTracker NextTracker { get; set; }

        public RoomComponent_AirLock LastAirLock { get; private set; }
        public RoomComponent_AirLock NextAirLock { get; private set; }

        private Building_AirLock[] LastPathDoors { get; set; }
        private Building_AirLock[] NextPathDoors { get; set; }

        //
        public IntVec3 PredictedNextCell => CurrentPath.Peek(1);

        public Pawn Pawn => (Pawn) parent;
        public Pawn_PathFollower Pather => Pawn.pather;

        public bool HasPath => Pather.curPath != null;
        public bool IsActive => Pather.Moving && HasPath && (Pather.curPath.Found && Pather.curPath.inUse);
        public bool IsValidForTransitionCheck => IsActive && LastRoom != NextRoom;
        public bool Selected => Find.Selector.IsSelected(Pawn);

        public override void PostExposeData()
        {
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {

            }
        }

        public void Notify_NewPath(PawnPath path)
        {
            CurrentPath = path;

            UpdateRoomsOnPath();
        }

        public void Notify_StopDead()
        {
            lastSetupDirty = false;
            CurrentPath = null;

            LastRoom = NextRoom = null;
            LastTracker = NextTracker = null;
            LastAirLock = NextAirLock = null;

            LastPathDoors = NextPathDoors = null;
        }

        public void Notify_EnteredRoom(RoomTracker roomTracker)
        {
            //TRLog.Debug($"[{Pawn.NameShortColored}] Entered Room (IsSame: {currentRoom == roomTracker})");
            if (CurrentTracker == roomTracker) return;
            CurrentRoom = roomTracker.Room;
            CurrentTracker = roomTracker;
            CurrentAirlock = roomTracker.GetRoomComp<RoomComponent_AirLock>();

            if (CurrentPath is null) return;
            UpdateRoomsOnPath();
        }

        private void UpdateRoomsOnPath()
        {
            //
            if (CurrentPath == null) return;

            var pathNodes = CurrentPath.nodes;
            for (var i = 0; i < pathNodes.Count; i++)
            {
                var node = pathNodes[(pathNodes.Count - 1) - i];
                var newRoom = node.GetRoom(Pawn.Map);
                if (newRoom == null)
                {
                    TRLog.Warning($"{Pawn.NameFullColored} has path node with null room.");
                    continue;
                }
                if (newRoom.IsDoorway) continue;
                if (newRoom != CurrentRoom) continue;
                LastRoom = NextRoom = CurrentRoom;

                //Predict:
                for (int k = i; k < pathNodes.Count; k++)
                {
                    var nextNode = pathNodes[(pathNodes.Count - 1) - k];
                    var nextNewRoom = nextNode.GetRoom(Pawn.Map);
                    if (nextNewRoom == null)
                    {
                        TRLog.Warning($"{Pawn.NameFullColored} has predicted path node with null room.");
                        continue;
                    }
                    if (nextNewRoom.IsDoorway) continue;
                    if (nextNewRoom == newRoom) continue;

                    NextRoom = nextNewRoom;
                    break;
                }
                if (LastRoom == NextRoom) return;

                LastTracker = LastRoom.RoomTracker();
                NextTracker = NextRoom.RoomTracker();

                LastAirLock = LastTracker.GetRoomComp<RoomComponent_AirLock>();
                NextAirLock = NextTracker.GetRoomComp<RoomComponent_AirLock>();

                LastPathDoors = LastAirLock.AirLocksOnPath(pathNodes, Pawn);
                NextPathDoors = NextAirLock.AirLocksOnPath(pathNodes, Pawn);
                break;
            }

            /*
            pathNodes.RoomsAlongPath(ref currentRoomsOnPath, parent.Map, true, true);
            for (var i = 0; i < currentRoomsOnPath.Count; i++)
            {
                var airLockComp = currentRoomsOnPath[i].GetRoomComp<RoomComponent_AirLock>();
                var airLockDoors = airLockComp.AirLocksOnPath(fullPath, Pawn);
                pathedDoors.Add(currentRoomsOnPath[i], airLockDoors);
                if (i == (currentRoomsOnPath.Count - 1))
                {
                    nextRooms.Add(currentRoomsOnPath[i], currentRoomsOnPath[i]);
                    break;
                }
                nextRooms.Add(currentRoomsOnPath[i], currentRoomsOnPath[i + 1]);
            }

            if (nextRooms.TryGetValue(CurrentRoom, out Room nextRoom))
            {
                LastRoom = CurrentRoom;
                NextRoom = nextRoom;

                LastTracker = LastRoom.RoomTracker();
                NextTracker = NextRoom.RoomTracker();

                LastAirLock = LastTracker.GetRoomComp<RoomComponent_AirLock>();
                NextAirLock = NextTracker.GetRoomComp<RoomComponent_AirLock>();
            }
            */
        }

        public bool CanEnterNextRoom()
        {
            if (!IsValidForTransitionCheck) return true;
            return NextAirLock?.CanBeEnteredBy(Pawn, NextPathDoors) ?? true;
        }

        public bool CanLeaveCurrentRoom()
        {
            if (!IsValidForTransitionCheck) return true;
            return LastAirLock.CanBeLeftBy(Pawn, LastPathDoors, true);
        }

        private bool CanMove()
        {
            if (!IsValidForTransitionCheck) return true;
            if (!CanLeaveCurrentRoom()) return false;
            if (!CanEnterNextRoom()) return false;
            return true;
        }

        public bool CanSetupMoveIntoNextCell(IntVec3 nextCell)
        {
            if (!IsActive) return true;
            var predictedNextCell = PredictedNextCell;
            var airLockDoor = Pawn.Map.thingGrid.ThingAt<Building_AirLock>(predictedNextCell);

            if (airLockDoor != null && !CanMove())
            { 
                if (!CanMove())
                {
                    //Notify last setup failed
                    lastSetupDirty = true;
                    return false;
                }
            }
            return true;
        }

        public bool CanEnterNextCell(IntVec3 nextCell)
        {
            if (CurrentPath is {inUse: false}) Notify_StopDead();

            if (lastSetupDirty)
            {
                if (!CanMove())
                {
                    return false;
                }
                //If Pawn can finally move, we do the missing setup
                Pather.SetupMoveIntoNextCell();
                lastSetupDirty = false;
            }
            return true;
        }

        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra();
            /*
            StringBuilder sb = new StringBuilder();
            if (DebugSettings.godMode)
            {
                sb.AppendLine($"Cur Transition: [{LastRoom?.ID}]->[{NextRoom?.ID}]");
                sb.AppendLine($"Can Leave Current: {CanLeaveCurrentRoom()}");
                sb.AppendLine($"Can Enter Next: {CanEnterNextRoom()}");
            }
            return sb.ToString().TrimEndNewlines();
            */
        }

        public override void PostDraw()
        {
            /*
            if (DebugSettings.godMode && Selected && IsActive)
            {
                if (LastRoom == null && NextRoom == null) return;
                if (LastRoom == NextRoom)
                {
                    GenDraw.DrawFieldEdges(LastRoom.Cells.ToList(), Color.yellow);
                    return;
                }

                if (LastRoom == null) return;
                GenDraw.DrawFieldEdges(LastRoom.Cells.ToList(), Color.cyan);
                if (NextRoom == null) return;
                GenDraw.DrawFieldEdges(NextRoom.Cells.ToList(), Color.magenta);

                if (LastPathDoors?[0] != null)
                    GenDraw.DrawFieldEdges(new() {LastPathDoors[0].Position}, Color.blue);
                if (LastPathDoors?[1] != null)
                    GenDraw.DrawFieldEdges(new() {LastPathDoors[1].Position}, Color.green);

                if (NextPathDoors?[1] != null)
                    GenDraw.DrawFieldEdges(new() {NextPathDoors[1].Position}, Color.red);
            }
            */
        }
    }

    public class CompProperties_PathFollowerExtra : CompProperties
    {
        public CompProperties_PathFollowerExtra()
        {
            this.compClass = typeof(Comp_PathFollowerExtra);
        }
    }
}
