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

    public class RoomComponent_AirLock : RoomComponent
    {
        private RoomComponent_Atmospheric atmosphericCompInt;
        private List<Building> AirVents = new List<Building>();
        private List<Building_AirLock> AirLockDoors = new List<Building_AirLock>();

        private Queue<Pawn> pawnQueue = new Queue<Pawn>();
        private HashSet<IntVec3> queuePosCells = new HashSet<IntVec3>();

        private int airVentCount, airLockDoorCount;
        private bool HasAirLockRole;

        public RoomComponent_Atmospheric Atmospheric => atmosphericCompInt ??= Parent.GetRoomComp<RoomComponent_Atmospheric>();

        public Pawn CurrentPawn
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
        public Queue<Pawn> PawnQueue => pawnQueue;
        public HashSet<IntVec3> ReservedQueue => queuePosCells;

        public bool IsValidBuffer => airLockDoorCount >= 2;
        public bool IsValid => airVentCount >= 1 && IsValidBuffer;
        public bool IsActive => IsValid && AirVents.Concat(AirLockDoors).All(c => c.IsPoweredOn());
        public bool IsClean => Atmospheric.UsedValue <= 0;

        public bool RequiresWait => AirLockDoors.Any(t => t.NeedsToClose);

        public bool AllDoorsClosed => !AirLockDoors.Any(d => d.Open);

        public AirLockUsage CurrentUsage
        {
            get
            {
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
        public bool IsReadyForUsage => IsClean && !IsBeingUsed;

        public bool ShouldBeUsed(bool isCurrent)
        {
            //Log.Message($"Data[{Room.ID}] IsActive:{IsActive} IsClean:{IsClean} IsValidBuffer:{IsValidBuffer} RequiresWait:{RequiresWait}");
            var usage = CurrentUsage;
            if (usage == AirLockUsage.None) return false;

            if (usage == AirLockUsage.WaitForDoors && (isCurrent && AllDoorsClosed)) return false;

            return true;
        }

        public override void Create(RoomTracker parent)
        {
            base.Create(parent);
            /*
            Lord lord = LordMaker.MakeNewLord(Faction.OfPlayer, MakeAirLockJob(), Map, null);
            lord.SetJob(new LordJob_UseAirlock());
            */
        }

        private LordJob MakeAirLockJob()
        {
            return new LordJob_UseAirlock();
        }

        public override void Notify_Reused()
        {
            base.Notify_Reused();
            atmosphericCompInt = null;
        }

        public override void Disband(RoomTracker parent, Map map)
        {
            base.Disband(parent, map);
        }

        public override void FinalizeApply()
        {
            RegenerateData();
        }

        public Building_AirLock[] AirLocksOnPath(List<IntVec3> pathNodes)
        {
            var airlocks = new Building_AirLock[2];
            TLog.Debug($"CurPath: {pathNodes != null} Nodes: {pathNodes?.Count} BorderCells: {Room.BorderCells.Count()}");
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

        public void Notify_EnqueuePawnPos(IntVec3 pos)
        {
            TLog.Debug($"Enqueued PawnPos: {pos}");
            queuePosCells.Add(pos);
        }

        public void Notify_DequeuePawnPos(IntVec3 pos)
        {
            TLog.Debug($"Dequeuing PawnPos: {pos}");
            queuePosCells.Remove(pos);
        }

        public void Notify_EnqueuePawn(Pawn pawn)
        {
            TLog.Debug($"Enqueued Pawn: {pawn}");
            pawnQueue.Enqueue(pawn);
        }

        public void Notify_FinishJob(Pawn pawn)
        {
            if (PawnQueue.TryDequeue(out Pawn deqPawn))
            {
                if (deqPawn != null && deqPawn != pawn)
                {
                    TLog.Error($"Trying to dequeue {pawn} from airlock queue, next should be: {CurrentPawn}");
                }
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

        private void RegenerateData()
        {
            //Get all bordering structures (airlocks..)
            AirVents.Clear();
            AirLockDoors.Clear();
            airLockDoorCount = airVentCount = 0;

            if (Room.Role != TiberiumDefOf.TR_AirLock)
            {
                for (var c = 0; c < Parent.BorderCellsNoCorners.Length; c++)
                {
                    var cell = Parent.BorderCellsNoCorners[c];
                    var things = cell.GetThingList(Map);
                    for (var t = 0; t < things.Count; t++)
                    {
                        TryAddComponent(things[t]);
                    }
                }
                return;
            }

            //Get all contained structures (vents..)
            HasAirLockRole = Room.Districts.All(r => r.Room.Role == TiberiumDefOf.TR_AirLock);
            for (var i = Room.ContainedAndAdjacentThings.Count - 1; i >= 0; i--)
            {
                var thing = Room.ContainedAndAdjacentThings[i];
                TryAddComponent(thing);
            }
        }

        private void TryAddComponent(Thing thing)
        {
            var comp = thing.TryGetComp<Comp_ANS_AirVent>();
            if (comp != null)
            {
                AirVents.Add(thing as Building);
                airVentCount++;
            }

            if (thing is Building_AirLock airLock)
            {
                AirLockDoors.Add(airLock);
                airLock.SetAirlock(this);
                airLockDoorCount++;
            }
        }

        private void TryRemoveComponent(Thing thing)
        {
            var comp = thing.TryGetComp<Comp_ANS_AirVent>();
            if (comp != null)
            {
                AirVents.Remove(thing as Building);
                airVentCount--;
            }
            if (thing is Building_AirLock airLock)
            {
                AirLockDoors.Remove(thing as Building_AirLock);
                airLockDoorCount--;
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

            foreach (var queuePosCell in queuePosCells)
            {
                GenDraw.DrawCircleOutline(queuePosCell.ToVector3Shifted(), 0.5f, SimpleColor.Green);
            }
        }
    }
}