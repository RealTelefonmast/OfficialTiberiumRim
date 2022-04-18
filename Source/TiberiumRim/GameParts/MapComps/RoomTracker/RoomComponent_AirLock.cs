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
using Verse.AI.Group;

namespace TiberiumRim
{
    public enum AirLockUsage
    {
        None,
        WaitForReady,
        WaitForDoors,
        WaitForClean
    }

    public class PawnQueue
    {
        public List<Pawn> pawnList;

        public PawnQueue()
        {
            pawnList = new();
        }

        public void Enqueue(Pawn pawn)
        {
            pawnList.Add(pawn);
        }

        public Pawn Dequeue()
        {
            TLog.Debug($"Queue Before Dequeing: {pawnList.ToStringSafeEnumerable()}");
            if (pawnList.Count <= 0) return null;
            var pawn = pawnList[0];
            pawnList.RemoveAt(0);
            TLog.Debug($"Queue After Dequeing {pawn.NameShortColored}: {pawnList.ToStringSafeEnumerable()}");
            return pawn;
        }

        public void Remove(Pawn pawn)
        {
            TLog.Debug($"Queue Before Removing: {pawnList.ToStringSafeEnumerable()}");
            if (pawnList.Remove(pawn))
            {
                TLog.Debug($"Safely removed {pawn.NameShortColored} from queue: {pawnList.ToStringSafeEnumerable()}");
            }
        }

        public bool TryPeek(out Pawn pawn)
        {
            pawn = null;
            if (pawnList.Count <= 0) return false;
            pawn = pawnList[0];
            return true;
        }

        public override string ToString()
        {
            return $"{pawnList.ToStringSafeEnumerable()}";
        }

        public bool Contains(Pawn pawn)
        {
            return pawnList.Contains(pawn);
        }
    }

    public class RoomComponent_AirLock : RoomComponent
    {
        private bool HasAirLockRole;

        private RoomComponent_Atmospheric atmosphericCompInt;
        private HashSet<Building> AirVents = new ();
        private HashSet<Building_AirLock> AirLockDoors = new ();

        private PawnQueue pawnQueue = new();
        private Dictionary<Pawn, IntVec3> queuePositions = new();
        //private HashSet<IntVec3> queuePosCells = new HashSet<IntVec3>();

        public RoomComponent_Atmospheric Atmospheric => atmosphericCompInt ??= Parent.GetRoomComp<RoomComponent_Atmospheric>();

        public PawnQueue PawnQueue => pawnQueue;
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

        public List<IntVec3> ReservedQueue => queuePositions.Values.ToList();

        public bool IsValidBuffer => AirLockDoors.Count >= 2;
        public bool IsValid => AirVents.Count >= 1 && IsValidBuffer;
        public bool IsActive => IsValid && AirVents.Concat(AirLockDoors).All(c => c.IsPoweredOn());
        public bool IsClean => Atmospheric.UsedValue <= 0;

        public bool RequiresWait => AirLockDoors.Any(t => t.CannotOpen);
        public bool AllDoorsClosed => !AirLockDoors.Any(d => d.Open);

        public bool ActiveReadyAirlock
        {
            get
            {
                if (!IsActive) return true;
                return IsActive && IsClean && AllDoorsClosed;
            }
        }

        public AirLockUsage CurrentUsage
        {
            get
            {
                if (!IsValid) return AirLockUsage.None;
                if (IsActive && !IsClean)
                {
                    return AirLockUsage.WaitForClean;
                }
                if (IsValidBuffer && RequiresWait)
                {
                    return AirLockUsage.WaitForDoors;
                }
                return AirLockUsage.None;
            }
        }

        public bool IsBeingUsed => CurrentUsage != AirLockUsage.None && containedPawns.Count > 0;
        public bool IsReadyForUsage => IsClean && !IsBeingUsed && !RequiresWait;

        public bool ShouldBeUsed(bool isCurrent, Pawn byPawn)
        {
            TLog.Debug($"[{byPawn.NameShortColored}][{Room.ID}]Data- IsActive:{IsActive} IsClean:{IsClean} IsValidBuffer:{IsValidBuffer} RequiresWait:{RequiresWait}");
            var usage = CurrentUsage;
            if (usage == AirLockUsage.None) return false;

            if (usage == AirLockUsage.WaitForDoors && (isCurrent && AllDoorsClosed)) return false;

            return true;
        }

        private LordJob MakeAirLockJob()
        {
            return new LordJob_UseAirlock();
        }

        //Queue Stuff
        public void Notify_EnqueuePawnPos(Pawn pawn, IntVec3 pos)
        {
            TLog.Debug($"[{pawn.NameShortColored}][{Room.ID}]Enqueued PawnPos: {pos}");
            queuePositions.Add(pawn, pos);
            //queuePosCells.Add(pos);
        }

        public void Notify_DequeuePawnPos(Pawn pawn, IntVec3 pos)
        {
            TLog.Debug($"[{Room.ID}]Dequeuing PawnPos: {pos}");
            queuePositions.Remove(pawn);
            //queuePosCells.Remove(pos);
        }

        public void Notify_EnqueuePawn(Pawn pawn)
        {
            TLog.Debug($"[{pawn.NameShortColored}][{Room.ID}]Enqueued Pawn: {pawn.NameShortColored}");
            pawnQueue.Enqueue(pawn);
        }

        public void Notify_FinishJob(Pawn pawn, JobCondition condition)
        {
            if (condition == JobCondition.Ongoing) return;
            if (condition != JobCondition.Succeeded)
            {
                pawnQueue.Remove(pawn);
                return;
            }
            if (pawnQueue.TryPeek(out Pawn deqPawn))
            {
                TLog.Debug($"[{pawn.NameShortColored}][{Room.ID}]Next Dequeued Pawn: {deqPawn?.NameShortColored}");
                if (deqPawn != null && deqPawn != pawn)
                {
                    TLog.Error($"Trying to dequeue {pawn} from airlock queue, next should be: {deqPawn} | Queue: {PawnQueue}");
                }
                else
                {
                    pawnQueue.Dequeue();
                }
            }
        }

        //Pathing Helper
        public Building_AirLock[] AirLocksOnPath(List<IntVec3> pathNodes, Pawn pawn = null)
        {
            var airlocks = new Building_AirLock[2];
            TLog.Debug($"[{pawn?.NameShortColored}]CurPath: {pathNodes != null} Nodes: {pathNodes?.Count} BorderCells: {Room.BorderCells.Count()}");
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
            if (Room.Role != TiberiumDefOf.TR_AirLock) return;

            //If we know this is an airlock, we add the rest of the internal items (Gettings vents)
            HasAirLockRole = Room.Districts.All(r => r.Room.Role == TiberiumDefOf.TR_AirLock);
            for (var i = Room.ContainedAndAdjacentThings.Count - 1; i >= 0; i--)
            {
                var thing = Room.ContainedAndAdjacentThings[i];
                TryAddComponent(thing);
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
                if (AirVents.Add(thing as Building))
                {
                    //
                }
            }

            if (thing is Building_AirLock airLock)
            {
                if (AirLockDoors.Add(airLock))
                {
                    airLock.SetAirlock(this);
                }
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
            if (UI.MouseCell().GetRoom(Map) == this.Room && HasAirLockRole) 
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