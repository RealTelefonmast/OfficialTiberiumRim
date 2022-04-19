using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
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

        private PawnQueue pawnQueue = new();
        public Dictionary<Pawn, IntVec3> queuePositions = new();

        //
        public RoomComponent_Atmospheric Atmospheric => atmosphericCompInt ??= Parent.GetRoomComp<RoomComponent_Atmospheric>();
        public PawnQueue PawnQueue => pawnQueue;
        public List<IntVec3> ReservedQueue => queuePositions.Values.ToList();

        public Pawn NextPawnInQueue
        {
            get
            {
                if (pawnQueue.TryPeek(out Pawn pawn))
                {
                    return pawn;
                }
                return null;
            }
        }

        //States
        //A room with two airlock doors, incapable of cleaning
        public bool IsBuffer => AirLockDoors.Count >= 2;

        //A room with airlock doors and a vent, capable of clearing the area
        public bool IsAirLock => AirVents.Count >= 1 && IsBuffer;

        //
        public bool IsActiveAirLock => IsAirLock && AirVents.Concat(AirLockDoors).All(c => c.IsPoweredOn());
        public bool IsClean => Atmospheric.UsedValue <= 0;
        public bool AllDoorsClosed => !AirLockDoors.Any(d => d.Open);
        public bool IsAllowedUsable => (!IsAirLock || CanVent);

        //Conditions
        public bool PollutedRoomExposure => AirLockDoors.Any(d => d.ConnectsToPollutedRoom);
        public bool SafeToEnter => !PollutedRoomExposure || AllDoorsClosed;
        public bool SafeToLeave => IsClean && AllDoorsClosed;

        public bool CanVent => IsActiveAirLock && AirVentComps.All(c => c.CanVent);
        public bool LockedDown => IsAirLock && !CanVent;

        public bool IsBeingUsedByOther(Pawn checkingPawn) => containedPawns.Count > 0 && !containedPawns.Contains(checkingPawn);
        public bool CanBeEnteredBy(Pawn pawn)
        {
            return IsClean && !IsBeingUsedByOther(pawn) && SafeToEnter;
        }

        public bool ShouldBeUsed(Pawn byPawn, Building_AirLock[] pathedDoors, bool isCurrentRoomOfPawn)
        {
            //If pawn is inside airlock, and all doors are closed, they dont need to use the airlock
            if (!IsAllowedUsable) return false;
            if (isCurrentRoomOfPawn && SafeToLeave) return false;
            return IsActiveAirLock && pathedDoors.Any(d => d?.ConnectsToPollutedRoom ?? false);
            //return true;
        }

        public bool ShouldWaitFor(Pawn pawn)
        {
           return !(CanBeEnteredBy(pawn) && (NextPawnInQueue == null || NextPawnInQueue == pawn));
        }

        //Queue Stuff
        public void Notify_EnqueuePawnPos(Pawn pawn, IntVec3 pos)
        {
            if (queuePositions.ContainsKey(pawn))
            {
                queuePositions[pawn] = pos;
                return;
            }
            queuePositions.Add(pawn, pos);
        }

        public void Notify_DequeuePawnPos(Pawn pawn)
        {
            if (!queuePositions.ContainsKey(pawn)) return;
            queuePositions.Remove(pawn);
        }

        public void Notify_EnqueuePawn(Pawn pawn)
        {
            pawnQueue.Enqueue(pawn);
        }

        public void Notify_FinishJob(Pawn pawn, JobCondition condition)
        {
            //if (condition == JobCondition.Ongoing) return;
            if (condition != JobCondition.Succeeded)
            {
                pawnQueue.Remove(pawn);
            }
            else if (pawnQueue.TryPeek(out Pawn deqPawn))
            {
                if (deqPawn != null && deqPawn != pawn)
                {
                    TLog.Error($"Trying to dequeue {pawn} from airlock queue, next should be: {deqPawn} | Queue: {PawnQueue}");
                }
                else
                {
                    pawnQueue.Dequeue();
                }
            }
            Notify_DequeuePawnPos(pawn);
        }

        //Pathing Helper
        public Building_AirLock[] AirLocksOnPath(List<IntVec3> pathNodes, Pawn pawn = null)
        {
            var airlocks = new Building_AirLock[2];
            var pathCells = pathNodes.Intersect(Room.BorderCells);
            int i = 0;
            foreach (var cell in pathCells)
            {
                var building = cell.GetEdifice(Map);
                if (building is Building_AirLock airlock)
                {
                    airlocks[i] = airlock;
                    i++;
                }
            }
            return airlocks;
        }

        public int tickSinceLastFleck = 0;
        public override void CompTick()
        {
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
            /*
            Lord lord = LordMaker.MakeNewLord(Faction.OfPlayer, MakeAirLockJob(), Map, null);
            lord.SetJob(new LordJob_UseAirlock());
            */
        }

        public override void Disband(RoomTracker parent, Map map)
        {
            base.Disband(parent, map);
        }

        public override void Notify_Reused()
        {
            base.Notify_Reused();
            atmosphericCompInt = null;
        }
        public override void PreApply()
        {
            AirVents.Clear();
            AirLockDoors.Clear();

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
            Room.UpdateRoomStatsAndRole();
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
        }

        public override void Notify_ThingSpawned(Thing thing)
        { 
            TryAddComponent(thing);
        }

        public override void Notify_ThingDespawned(Thing thing)
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
            if (UI.MouseCell().GetRoom(Map) == this.Room && hasAirLockRoleInt) 
            {
                GenDraw.DrawCircleOutline(Room.GeneralCenter().ToVector3Shifted(), 0.5f, SimpleColor.Red);
                GenDraw.DrawFieldEdges(AirLockDoors.Select(t => t.Position).ToList(), Color.blue);
                GenDraw.DrawFieldEdges(AirVents.Select(t => t.Position).ToList(), Color.green);

            }

            foreach (var queuePosCell in ReservedQueue)
            {
                GenDraw.DrawCircleOutline(queuePosCell.ToVector3Shifted(), 0.5f, SimpleColor.Green);
            }
        }

        public bool AlreadyWaitingFor(Pawn pawn, out IntVec3 value)
        {
            return queuePositions.TryGetValue(pawn, out value) && value.IsValid;
        }
    }
}