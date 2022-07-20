using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using TeleCore;
using TiberiumRim.Utilities;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public enum AirLockUsage
    {
        None,
        WaitForReady,
        WaitForDoors,
        WaitForClean
    }

    public class RoomComponent_AirLock : RoomComponent
    {
        //
        private bool hasAirLockRoleInt;
        private RoomComponent_Atmospheric atmosphericCompInt;

        //
        private HashSet<Comp_ANS_AirVent> AirVentComps = new();
        private HashSet<Building> AirVents = new ();
        private HashSet<Building_AirLock> AirLockDoors = new ();

        public Dictionary<Pawn, IntVec3> queuePositions = new();

        //
        public RoomComponent_Atmospheric Atmospheric => atmosphericCompInt ??= Parent.GetRoomComp<RoomComponent_Atmospheric>();

        //States
        //A room with two airlock doors, incapable of cleaning
        public bool IsBuffer => AirLockDoors.Count >= 2;

        //A room with airlock doors and a vent, capable of clearing the area
        public bool IsAirLock => AirVents.Count >= 1 && IsBuffer;

        //
        public bool IsActiveAirLock => IsAirLock && AirVents.Concat(AirLockDoors).All(c => c.IsPoweredOn());

        //Conditions
        public bool IsFunctional => (!IsAirLock || CanVent);
        public bool IsClean => Atmospheric.UsedValue <= 0 && Atmospheric.PhysicalGas.NullOrEmpty();
        public bool IsBeingCleaned => !IsClean && CanVent;

        public bool CanVent => IsActiveAirLock && AirVentComps.All(c => c.CanVent);
        public bool LockedDown => IsAirLock && !CanVent;

        public bool AllDoorsClosed => !AirLockDoors.Any(d => d.Open);
        public bool PollutedRoomExposure => AirLockDoors.Any(d => d.ConnectsToPollutedRoom);

        public bool AnyPollutedDoorOpening => AirLockDoors.Where(d => d.ConnectsToPollutedRoom).Any(d => d.Open);

        public bool CanBeEnteredBy(Pawn pawn, Building_AirLock[] pathedDoors)
        {
            //Case #1: Normal room, can enter always
            if (!IsActiveAirLock) return true;

            //Case #2: Active Airlock not clean, cant enter
            if (!IsClean)
            {
                //Case #2.1: But connecting to polluted room anyway
                if (!pathedDoors[0].OtherIsClean(this)) return true;
                return false;
            }

            //If clean, but entering through polluted door, all doors must be closed first
            if (pathedDoors[0].ConnectsToPollutedRoom && !AllDoorsClosed) return false;

            //Case #3: Door connecting to pollution is opening
            if (AnyPollutedDoorOpening) return false;
            return true;
        }

        public bool CanBeLeftBy(Pawn pawn, Building_AirLock[] pathedDoors, bool isCurrentRoomOfPawn)
        {
            //Case #1: Normal room, can leave always
            if (!IsActiveAirLock) return true;

            //Case #2: Active airlock not clean, waiting for venting
            if (!IsClean)
            {
                //Case #2.1: But connecting to polluted room anyway
                if (!pathedDoors[1].OtherIsClean(this)) return true;
                return false;
            }

            //Case #3: Door to next room is not connecting to pollution
            if (!pathedDoors[1].ConnectsToPollutedRoom)
            {
                //Case #3.1: But other doors opening to pollution
                if (AnyPollutedDoorOpening) return false;
                return true;
            }

            //
            if (pathedDoors[1].ConnectsToPollutedRoom && AllDoorsClosed) return true;
            return false;
        }

        //Pathing Helper
        // [0] Entrance | [1] Exit
        public Building_AirLock[] AirLocksOnPath(List<IntVec3> pathNodes, Pawn pawn = null)
        {
            var airlocks = new Building_AirLock[2];
            Room roomIn = null;
            Room roomOut = null;
            for (int i = 0; i < pathNodes.Count; i++)
            {
                var nextNode = pathNodes[i];
                if (Parent.BorderCellsNoCorners.Contains(nextNode))
                {
                    var building = nextNode.GetEdifice(Map);
                    if (building is Building_AirLock airlock)
                    {
                        //Try set both sides of the airlock door
                        if(i + 1 < pathNodes.Count)
                            roomIn = pathNodes[i + 1].GetRoomFast(Map);
                        if (i - 1 >= 0)
                            roomOut = pathNodes[i - 1].GetRoomFast(Map);
                
                        //If airlock door is at start or end of the path, predict next room
                        if (i + 1 >= pathNodes.Count)
                            roomIn = airlock.OppositeRoom(roomOut);
                        if (i - 1 < 0)
                            roomOut = airlock.OppositeRoom(roomIn);
                        
                        //Set airlock doors depending on order of rooms
                        if (roomIn != Room && roomOut == Room)
                        {
                            airlocks[0] = airlock;
                        }

                        if (roomIn == Room && roomOut != Room)
                        {
                            airlocks[1] = airlock;
                        }
                    }
                }
            }

            return airlocks;
        }

        public int tickSinceLastFleck = 0;
        public override void CompTick()
        {
            if (!IsAirLock) return;

            AirLockDoors.Do(d => d.CheckLockDown(LockedDown));
            if (LockedDown)
            {
                if (tickSinceLastFleck <= 0)
                {
                    foreach (var airLock in AirLockDoors)
                    {
                        FleckMaker.ThrowMetaIcon(airLock.Position, airLock.Map, FleckDefOf.IncapIcon, 0.21f);
                    }
                    tickSinceLastFleck = 200;
                }
                tickSinceLastFleck--;
            }
        }

        //RoomComponent Stuff
        public override void Create(RoomTracker parent)
        {
            base.Create(parent);
        }

        public override void Disband(RoomTracker parent, Map map)
        {
            base.Disband(parent, map);
        }

        public override void Notify_Reused()
        {
            atmosphericCompInt = null;
        }

        public override void PreApply()
        {
            AirVents.Clear();
            AirLockDoors.Clear();

            if(Parent.IsOutside) return;
            
            //Pre-gen data by only checking bordering cells (Getting Airlocks)
            for (var c = 0; c < Parent.BorderCellsNoCorners.Length; c++)
            {
                var cell = Parent.BorderCellsNoCorners[c];
                var things = cell.GetThingList(Map);
                for (var t = 0; t < things.Count; t++)
                {
                    TryAddComponent(things[t]);
                }
            }
        }

        public override void FinalizeApply()
        {
            //TRLog.Debug($"Updating AirLockComp for [{Room.ID}][Pre]: IsOutside: {Parent.IsOutside} | Role: {Room.Role}");
            if (Parent.IsOutside) return;
            Room.UpdateRoomStatsAndRole();
            //TRLog.Debug($"Updating AirLockComp for [{Room.ID}][Post]: Role: {Room.Role}");
            if (Room.Role == TiberiumDefOf.TR_AirLock)
            {
                //If we know this is an airlock, we add the rest of the internal items (Gettings vents)
                hasAirLockRoleInt = Room.Districts.All(r => r.Room.Role == TiberiumDefOf.TR_AirLock);
                for (var i = Room.ContainedAndAdjacentThings.Count - 1; i >= 0; i--)
                {
                    var thing = Room.ContainedAndAdjacentThings[i];
                    TryAddComponent(thing);
                }
            }

            //Set AirLockDoor Data after all local data has been generated
            foreach (var airLockDoor in AirLockDoors)
            {
                airLockDoor.SetAirlock(this);
            }
            //TRLog.Debug($"[StopWatch][RoomComp_AirLock]FinalizeApply: {_SW.ElapsedMilliseconds}");
        }

        public override void Notify_ThingAdded(Thing thing)
        { 
            TryAddComponent(thing);
        }

        public override void Notify_ThingRemoved(Thing thing)
        {
            TryRemoveComponent(thing);
        }

        private void TryAddComponent(Thing thing)
        {
            var comp = thing.TryGetComp<Comp_ANS_AirVent>();
            if (comp != null)
            {
                if (AirVents.Add(thing as Building) && AirVentComps.Add(comp))
                {
                    comp.SetAirLock(this);
                }
            }

            if (thing is Building_AirLock airLock)
            {
                AirLockDoors.Add(airLock);
            }
        }

        private void TryRemoveComponent(Thing thing)
        {
            var comp = thing.TryGetComp<Comp_ANS_AirVent>();
            if (comp != null)
            {
                AirVents.Remove(thing as Building);
            }
            if (thing is Building_AirLock airLock)
            {
                AirLockDoors.Remove(airLock);
            }
        }

        public override void Draw()
        {
            if (DebugSettings.godMode && hasAirLockRoleInt && UI.MouseCell().GetRoom(Map) == this.Room) 
            {
                GenDraw.DrawCircleOutline(Room.GeneralCenter().ToVector3Shifted(), 0.5f, SimpleColor.Red);
                GenDraw.DrawFieldEdges(AirLockDoors.Select(t => t.Position).ToList(), Color.blue);
                GenDraw.DrawFieldEdges(AirVents.Select(t => t.Position).ToList(), Color.green);
            }
        }
    }
}