using System;
using System.Collections.Generic;
using System.Linq;
using TeleCore.Logging;
using TeleCore.Static;
using TeleCore.Utility;
using TeleCore.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Systems.RoomTracking;

public class RoomTracker
{
    private const char check = '✓';
    private const char fail = '❌';
    internal static readonly List<Type> RoomComponentTypes;

    //
    internal bool DebugSelected;
    internal bool DebugPortals;

    //
    private bool _wasOutSide;
    private HashSet<IntVec3> borderCells = new();
    private HashSet<IntVec3> thinRoofCells = new();
    private HashSet<Room> borderRooms = new();
    private readonly Dictionary<Type, RoomComponent> _compsByType = new();

    private RoomNeighborSet _roomNeighbors = new();

    public IEnumerable<RoomComponent> Comps => _compsByType.Values;
    public RoomNeighborSet RoomNeighbors => _roomNeighbors;

    static RoomTracker()
    {
        RoomComponentTypes = typeof(RoomComponent).AllSubclassesNonAbstract();
    }

    public RoomTracker(Room room)
    {
        Room = room;
        ListerThings = new ListerThings(ListerThingsUse.Region);
        BorderListerThings = new ListerThings(ListerThingsUse.Region);

        _roomNeighbors = new RoomNeighborSet();

        //Get Group Data
        UpdateRoomData();

        //Create Components
        foreach (var type in RoomComponentTypes)
        {
            var comp = (RoomComponent)Activator.CreateInstance(type);
            comp.Create(this);
            _compsByType.Add(type, comp);
        }

        foreach (var comp in Comps)
        {
            comp.PostCreate(this);
        }
    }

    #region Properties

    public Verse.Map Map { get; private set; }
    public bool IsDisbanded { get; private set; }
    public bool IsOutside { get; private set; }
    public bool IsProper { get; private set; }
    public int CellCount { get; private set; }
    public int OpenRoofCount { get; private set; }

    public Room Room { get; }

    public ListerThings ListerThings { get; }
    public ListerThings BorderListerThings { get; }

    public IReadOnlyCollection<Thing> ContainedPawns => ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);

    public RoomTrackerLink SelfTrackerLink
    {
        get;
        private set;
    }

    public RegionType RegionTypes { get; private set; }

    public IReadOnlyCollection<IntVec3> BorderCellsNoCorners => borderCells;
    public IReadOnlyCollection<IntVec3> ThinRoofCells => thinRoofCells;

    public IntVec3[] MinMaxCorners { get; private set; } = new IntVec3[4];

    public IntVec3 MinVec { get; private set; }

    public IntVec2 Size { get; private set; }

    public Vector3 ActualCenter { get; private set; }

    public Vector3 DrawPos { get; private set; }

    #endregion


    #region Room State Change

    public void Init(RoomTracker?[] previous)
    {
        //Try get self portal
        if (Room.IsDoorway)
        {
            var door = Room.Regions[0].door;
            var roomFacing = (door.Rotation.FacingCell + door.Position).GetRoom(Map)?.RoomTracker();
            var roomOpposite = (door.Rotation.Opposite.FacingCell + door.Position).GetRoom(Map)?.RoomTracker();
            if (roomFacing == null || roomOpposite == null)
            {
                SelfTrackerLink = new RoomTrackerLink(door, this);
            }
            else
            {
                SelfTrackerLink = new RoomTrackerLink(Room.Regions[0].door, roomFacing, roomOpposite, this);
            }
        }

        //
        RegenerateData();
        foreach (var comp in Comps)
        {
            comp.InternalInit(previous);
        }
    }

    public void PostInit(RoomTracker?[] previous)
    {
        foreach (var comp in Comps)
            comp.PostInit(previous);
    }

    public void Reset()
    {
        _roomNeighbors.Reset();

        foreach (var comp in Comps)
            comp.Reset();
    }

    public void Notify_Reused()
    {
        RegenerateData(true);
        foreach (var comp in Comps)
            comp.Notify_Reused();
    }

    public void MarkDisbanded()
    {
        IsDisbanded = true;
    }

    public void Disband(Verse.Map onMap)
    {
        _roomNeighbors.Reset();

        foreach (var comp in Comps)
        {
            comp.DisbandInternal();
            comp.Disband(this, onMap);
        }
    }

    public void Notify_RoofChanged()
    {
        RegenerateData(true, false, false);
        //Check if room closed
        if (_wasOutSide && !IsOutside)
            RoofClosed();

        if (!_wasOutSide && IsOutside)
            RoofOpened();

        foreach (var comp in Comps)
            comp.Notify_RoofChanged();
    }

    private void RoofClosed()
    {
        foreach (var comp in Comps)
            comp.Notify_RoofClosed();
    }

    private void RoofOpened()
    {
        foreach (var comp in Comps)
            comp.Notify_RoofOpened();
    }

    #endregion

    #region Room Thing Data

    public void Notify_RegisterThing(Thing thing)
    {
        //Things
        ListerThings.Add(thing);
        foreach (var comp in Comps)
        {
            comp.Notify_ThingAdded(thing);
            if (thing is Pawn pawn)
            {
                //Entered room
                //var followerExtra = pawn.GetComp<Comp_PathFollowerExtra>();
                //followerExtra?.Notify_EnteredRoom(this);
                comp.Notify_PawnEnteredRoom(pawn);
            }
        }
    }

    public void Notify_DeregisterThing(Thing thing)
    {
        //Things
        ListerThings.Remove(thing);
        foreach (var comp in Comps)
        {
            comp.Notify_ThingRemoved(thing);
            if (thing is Pawn pawn)
                comp.Notify_PawnLeftRoom(pawn);
        }
    }

    public void Notify_PawnEnteredRoom(Pawn pawn)
    {
        foreach (var comp in Comps)
        {
            comp.Notify_PawnEnteredRoom(pawn);
        }
    }

    public void Notify_RegisterBorderThing(Thing thing)
    {
        if (BorderListerThings.Contains(thing)) return;

        HandleAttachedNghb(thing);
        BorderListerThings.Add(thing);
        foreach (var comp in Comps)
        {
            comp.Notify_HandleBorderThing(thing);
            //_neighbors.TrueNeighbors.Do(a => comp.AddAdjacent(a._compsByType[comp.GetType()]));
        }
    }

    private void HandleAttachedNghb(Thing thing)
    {
        if (thing is Building b)
        {
            //Add room on other side
            var otherRoom = b.NeighborRoomOf(Room);
            if (otherRoom == null) return;
            var otherTracker = otherRoom.RoomTracker();
            if (otherTracker == this) return;
            _roomNeighbors.Notify_AddAttachedNeighbor(otherTracker);
        }
    }

    #endregion

    public void FinalizeMapInit()
    {
        foreach (var comp in Comps)
            comp.FinalizeMapInit();
    }

    public RoomComponent GetRoomComp(Type type)
    {
        return _compsByType[type];
    }

    public T GetRoomComp<T>() where T : RoomComponent
    {
        return (T)_compsByType[typeof(T)];
    }

    public bool ContainsRegionType(RegionType type)
    {
        return RegionTypes.HasFlag(type);
    }

    public bool ContainsPawn(Pawn pawn)
    {
        return ContainedPawns.Contains(pawn);
    }

    public void RoomTick()
    {
        foreach (var comp in Comps)
            comp.CompTick();
    }

    public void RoomOnGUI()
    {
        foreach (var comp in Comps)
            comp.OnGUI();
    }

    public void RoomDraw()
    {
        //
        foreach (var comp in Comps)
            comp.Draw();

        if (!DebugSettings.godMode) return;

        //Debug
        if (DebugSelected)
        {
            GenDraw.DrawFieldEdges(Room.Cells.ToList(), Color.cyan);
            if (DebugPortals)
            {
                foreach (var attachedNghb in RoomNeighbors.AttachedNeighbors)
                {
                    GenDraw.DrawFieldEdges(attachedNghb.Room.Cells.ToList(), Color.magenta);
                }

                foreach (var trueNghb in RoomNeighbors.TrueNeighbors)
                {
                    GenDraw.DrawFieldEdges(trueNghb.Room.Cells.ToList(), Color.green);
                }
            }
        }
    }

    internal void DrawDebug()
    {
        GenDraw.DrawFieldEdges(Room.Cells.ToList(), Color.cyan);

        foreach (var comp in Comps)
        {
            comp.DrawDebug();
        }

        var debugList = StaticListHolder<IntVec3>.RequestList("RoomTracker.DrawDebug");
        debugList.AddRange(borderCells);
        GenDraw.DrawFieldEdges(debugList, Color.blue);
        debugList.Clear();
    }

    private string Icon(bool checkFail)
    {
        return $"{(checkFail ? check : fail)}";
    }

    private Color ColorSel(bool checkFail)
    {
        return checkFail ? Color.green : Color.red;
    }

    public void Validate()
    {
        TLog.Message($"### Validating Tracker[{Room.ID}]");
        var innerCells = Room.Cells;

        var containedThing = Room.ContainedAndAdjacentThings.Where(t => innerCells.Contains(t.Position)).ToList();
        var sameCount = containedThing.Count(t => ListerThings.Contains(t));
        var sameRatio = sameCount / (float)containedThing.Count();
        var sameBool = sameRatio >= 1f;
        var sameRatioString = $"[{sameCount}/{containedThing.Count()}][{sameRatio}]{Icon(sameBool)}".Colorize(ColorSel(sameBool));
        TLog.Message($"ContainedThingRatio: {sameRatioString}");
        TLog.Message($"### Ending Validation [{Room.ID}]");
    }

    public void RegenerateData(bool ignoreRoomExtents = false, bool regenCellData = true, bool regenListerThings = true)
    {
        var stepCounter = 0;
        try
        {
            UpdateRoomData();
            stepCounter = 1;
            if (!ignoreRoomExtents)
            {
                if (Room.Dereferenced)
                {
                    TLog.Warning($"Trying to regenerate dereferenced room. IsDisbanded: {IsDisbanded}");
                    return;
                }

                var extents = Room.ExtentsClose;
                var minX = extents.minX;
                var maxX = extents.maxX;
                var minZ = extents.minZ;
                var maxZ = extents.maxZ;
                MinMaxCorners = extents.Corners.ToArray();

                stepCounter = 2;
                MinVec = new IntVec3(minX, 0, minZ);
                Size = new IntVec2(maxX - minX + 1, maxZ - minZ + 1);
                ActualCenter = extents.CenterVector3;
                DrawPos = new Vector3(minX, AltitudeLayer.FogOfWar.AltitudeFor(), minZ);
                stepCounter = 3;
            }

            //Get Roof and Border Cells
            if (regenCellData)
            {
                stepCounter = 4;
                GenerateCellData();
            }

            stepCounter = 5;
            //Get ListerThings
            if (regenListerThings)
            {
                stepCounter = 6;
                RegenerateListerThings();
                RegenerateRoomRelations();
            }
        }
        catch (OverflowException oEx)
        {
            TLog.Error($"Arithmetic Overflow Exception in RegenerateData - reached step {stepCounter}: {oEx}");
        }
    }

    private void RegenerateRoomRelations()
    {
        foreach (var room in borderRooms)
        {
            var otherTracker = room.RoomTracker();
            if (otherTracker == this) continue;
            _roomNeighbors.Notify_AddNeighbor(otherTracker);
        }
    }

    private void RegenerateListerThings()
    {
        //var watch = new Stopwatch();
        //watch.Start();

        ListerThings.Clear();
        BorderListerThings.Clear();

        for (var r = 0; r < Room.Regions.Count; r++)
        {
            var region = Room.Regions[r];
            var pawns = region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
            foreach (var pawn in pawns)
            {
                Notify_PawnEnteredRoom((Pawn)pawn);
            }
        }

        foreach (var cell in borderCells)
        {
            var edi = cell.GetEdifice(Map);
            Notify_RegisterBorderThing(edi);
            continue;
            var things = cell.GetThingList(Map);
            foreach (var thing in things)
            {
                Notify_RegisterBorderThing(thing);
            }
        }

        //watch.Stop();
        //TLog.Message($"RegenerateListerThings took {watch.ElapsedMilliseconds}ms");
    }

    private void GenerateCellData()
    {
        //Caching cell data on outside room is performance intensive
        if (!IsProper) return;

        thinRoofCells.Clear();
        borderCells.Clear();
        borderRooms.Clear();
        foreach (var c in Room.Cells)
        {
            if (!Map.roofGrid.RoofAt(c)?.isThickRoof ?? false)
                thinRoofCells.Add(c);

            for (var i = 0; i < 4; i++)
            {
                var cardinal = c + GenAdj.CardinalDirections[i];

                var region = cardinal.GetRegion(Map, RegionType.Set_Passable);
                if (cardinal.InBounds(Map))
                {
                    var noReg = region == null;
                    var unsameRoom = !noReg && region.Room != Room;
                    if (noReg || unsameRoom)
                    {
                        if (unsameRoom)
                        {
                            borderRooms.Add(region.Room);
                        }
                        borderCells.Add(cardinal);
                    }
                }
            }
        }
    }

    private void UpdateRoomData()
    {
        _wasOutSide = IsOutside;
        IsOutside = Room.UsesOutdoorTemperature; //Caching to avoid recalc
        IsProper = Room.ProperRoom;
        CellCount = Room.CellCount;
        Map = Room.Map;

        if (!IsOutside)
        {
            //If not outside, we want to know if there are any open roof cells
            OpenRoofCount = Room.OpenRoofCount;
        }

        foreach (var roomRegion in Room.Regions)
            RegionTypes |= roomRegion.type;
    }

    public override string ToString()
    {
        return base.ToString();
    }

    internal void Debug_Select()
    {
        DebugSelected = true;
    }

    internal void Debug_Deselect()
    {
        DebugSelected = false;
        DebugPortals = false;
    }
}