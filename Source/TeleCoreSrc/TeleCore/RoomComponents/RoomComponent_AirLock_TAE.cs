// Preserved from TeleCore/Airlock/RoomComponent_AirLock.cs

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using TeleCore.Buildings;
using TeleCore.Comps;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

//
public class RoomComponent_AirLock_TAE : RoomComponent
{
    private readonly HashSet<Building_Airlock> AirLockDoors = new();
    private readonly HashSet<Comp_ANS_AirLockVent> AirVentComps = new();

    //
    private readonly HashSet<Building> AirVents = new();

    private RoomComponent_Atmosphere atmosphericCompInt;

    //
    private bool hasAirLockRoleInt;

    public int tickSinceLastFleck;

    //
    public RoomComponent_Atmosphere Atmospheric =>
        atmosphericCompInt ??= Parent.GetRoomComp<RoomComponent_Atmosphere>();

    //States
    public bool IsBuffer => AirLockDoors.Count >= 2;
    public bool IsAirLock => AirVents.Count >= 1 && IsBuffer;
    public bool IsActiveAirLock => IsAirLock && AirVents.Concat(AirLockDoors).All(c => c.IsPoweredOn());

    //Conditions
    public bool IsFunctional => !IsAirLock || CanVent;
    public bool IsClean => Atmospheric.Volume.TotalValue <= 0;
    public bool IsBeingCleaned => !IsClean && CanVent;

    public bool CanVent => IsActiveAirLock && AirVentComps.All(c => c.CanVent);
    public bool LockedDown => IsAirLock && !CanVent;

    public bool AllDoorsClosed => !AirLockDoors.Any(d => d.Open);
    public bool PollutedRoomExposure => AirLockDoors.Any(d => d.ConnectsToPollutedRoom);

    public bool AnyPollutedDoorOpening => AirLockDoors.Where(d => d.ConnectsToPollutedRoom).Any(d => d.Open);

    public bool CanBeEnteredBy(Pawn pawn, Building_Airlock[] pathedDoors)
    {
        if (!IsActiveAirLock) return true;
        if (!IsClean)
        {
            if (!pathedDoors[0].OtherIsClean(this)) return true;
            return false;
        }

        if (pathedDoors[0].ConnectsToPollutedRoom && !AllDoorsClosed) return false;
        if (AnyPollutedDoorOpening) return false;
        return true;
    }

    public bool CanBeLeftBy(Pawn pawn, Building_Airlock[] pathedDoors, bool isCurrentRoomOfPawn)
    {
        if (!IsActiveAirLock) return true;
        if (!IsClean)
        {
            if (!pathedDoors[1].OtherIsClean(this)) return true;
            return false;
        }

        if (!pathedDoors[1].ConnectsToPollutedRoom)
        {
            if (AnyPollutedDoorOpening) return false;
            return true;
        }

        if (pathedDoors[1].ConnectsToPollutedRoom && AllDoorsClosed) return true;
        return false;
    }

    public Building_Airlock[] AirLocksOnPath(List<IntVec3> pathNodes, Pawn pawn = null)
    {
        var airlocks = new Building_Airlock[2];
        Room roomIn = null;
        Room roomOut = null;
        for (var i = 0; i < pathNodes.Count; i++)
        {
            var nextNode = pathNodes[i];
            if (Parent.BorderCellsNoCorners.Contains(nextNode))
            {
                var building = nextNode.GetEdifice(Map);
                if (building is Building_Airlock airlock)
                {
                    if (i + 1 < pathNodes.Count)
                        roomIn = pathNodes[i + 1].GetRoomFast(Map);
                    if (i - 1 >= 0)
                        roomOut = pathNodes[i - 1].GetRoomFast(Map);

                    if (i + 1 >= pathNodes.Count)
                        roomIn = airlock.OppositeRoom(roomOut);
                    if (i - 1 < 0)
                        roomOut = airlock.OppositeRoom(roomIn);

                    if (roomIn != Room && roomOut == Room) airlocks[0] = airlock;

                    if (roomIn == Room && roomOut != Room) airlocks[1] = airlock;
                }
            }
        }

        return airlocks;
    }

    public override void CompTick()
    {
        if (!IsAirLock) return;

        AirLockDoors.Do(d => d.CheckLockDown(LockedDown));
        if (LockedDown)
        {
            if (tickSinceLastFleck <= 0)
            {
                foreach (var airLock in AirLockDoors)
                    FleckMaker.ThrowMetaIcon(airLock.Position, airLock.Map, FleckDefOf.IncapIcon, 0.21f);
                tickSinceLastFleck = 200;
            }

            tickSinceLastFleck--;
        }
    }

    public override void PostCreate(RoomTracker parent)
    {
        base.PostCreate(parent);
    }

    public override void Disband(RoomTracker parent, Map map)
    {
        base.Disband(parent, map);
    }

    public override void Notify_Reused()
    {
        atmosphericCompInt = null;
    }

    public override void Init(RoomTracker[] previous = null)
    {
        AirVents.Clear();
        AirLockDoors.Clear();

        if (Parent.IsOutside) return;

        foreach (var cell in Parent.BorderCellsNoCorners)
        {
            var things = cell.GetThingList(Map);
            for (var t = 0; t < things.Count; t++) TryAddComponent(things[t]);
        }
    }

    public override void PostInit(RoomTracker[] previous = null)
    {
        if (Parent.IsOutside) return;
        Room.UpdateRoomStatsAndRole();
        if (Room.Role == TAE.AtmosDefOf.TAE_AirLockRole)
        {
            hasAirLockRoleInt = Room.Districts.All(r => r.Room.Role == TAE.AtmosDefOf.TAE_AirLockRole);
            for (var i = Room.ContainedAndAdjacentThings.Count - 1; i >= 0; i--)
            {
                var thing = Room.ContainedAndAdjacentThings[i];
                TryAddComponent(thing);
            }
        }

        foreach (var airLockDoor in AirLockDoors) airLockDoor.SetAirlock(this);
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
        var comp = thing.TryGetComp<Comp_ANS_AirLockVent>();
        if (comp != null)
            if (AirVents.Add(thing as Building) && AirVentComps.Add(comp))
                comp.SetAirLock(this);

        if (thing is Building_Airlock airLock) AirLockDoors.Add(airLock);
    }

    private void TryRemoveComponent(Thing thing)
    {
        var comp = thing.TryGetComp<Comp_ANS_AirVent>();
        if (comp != null) AirVents.Remove(thing as Building);
        if (thing is Building_Airlock airLock) AirLockDoors.Remove(airLock);
    }

    public override void Draw()
    {
        if (DebugSettings.godMode && hasAirLockRoleInt && UI.MouseCell().GetRoom(Map) == Room)
        {
            GenDraw.DrawFieldEdges(AirLockDoors.Select(t => t.Position).ToList(), Color.blue);
            GenDraw.DrawFieldEdges(AirVents.Select(t => t.Position).ToList(), Color.green);
        }
    }
}