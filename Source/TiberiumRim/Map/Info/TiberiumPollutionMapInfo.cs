using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using TR.Interfaces;
using UnityEngine;
using Verse;
using Enumerable = System.Linq.Enumerable;

namespace TR.Info;

public class PollutionPasser
{
    public PollutionPasser(Building b)
    {
        Building = b;
    }

    public Building Building { get; }

    public bool CanPass
    {
        get
        {
            return Building switch
            {
                Building_Door door => door.Open,
                Building_Vent vent => FlickUtility.WantsToBeOn(vent),
                Building_Cooler cooler => cooler.IsPowered(out _),
                _ => false
            };
        }
    }
}

public class TiberiumPollutionMapInfo : MapInformation
{
    [TweakValue("Pollution_Ticks", 1f, 250f)]
    public static int Ticks = 100;

    public PollutionCache pollutionCache;
    public List<PollutionTracker> PollutionTrackers = new();

    public List<IPollutionSource> Sources = new();
    public PollutionRoomUpdater updater;

    public TiberiumPollutionMapInfo(Map map) : base(map)
    {
        updater = new PollutionRoomUpdater(this);
        pollutionCache = new PollutionCache(map);
    }

    public int OutsidePollution { get; set; }

    public override void ExposeDataExtra()
    {
        Scribe_Deep.Look(ref pollutionCache, "pollutionCache", map);
    }

    public PollutionTracker PollutionAt(IntVec3 pos)
    {
        return PollutionTrackers.Find(t => t.Group == pos.GetRoomGroup(map));
    }

    public PollutionTracker TrackerFor(Room room)
    {
        return PollutionTrackers.Find(t => t.Group == room.Group);
    }

    public void RegisterSource(IPollutionSource source)
    {
        if (Sources.Contains(source)) return;
        Sources.Add(source);
    }

    public void DeregisterSource(IPollutionSource source)
    {
        Sources.Remove(source);
    }

    public override void Tick()
    {
        if (Find.TickManager.TicksGame % Ticks == 10)
            foreach (var tracker in PollutionTrackers)
                tracker.Equalize();

        foreach (var source in Sources)
            if (source?.Thing?.IsHashIntervalTick(source.PollutionInterval) ?? false)
                Pollute(source);
    }

    private void Pollute(IPollutionSource source)
    {
        TrackerFor(source.Room).Pollute(source.PollutionAmount);
    }

    public float PollutionPercent(Room room)
    {
        return TrackerFor(room).Saturation;
    }

    public override void UpdateOnGUI()
    {
        foreach (var tracker in PollutionTrackers) tracker.OnGUI();
    }

    public override void Update()
    {
        if (!TRUtils.Tiberium().GameSettings.RadiationOverlay) return;
        foreach (var pollutionTracker in PollutionTrackers)
            /*
            foreach (var cell in pollutionTracker.Group.Cells)
            {
                CellRenderer.RenderCell(cell, pollutionTracker.Saturation);
            }*/
            pollutionTracker.DrawData();
    }
}

public struct CachedPollutionInfo
{
    public int roomGroupID;
    public int numCells;
    public int pollution;

    public CachedPollutionInfo(int roomGroupID, int numCells, int pollution)
    {
        this.roomGroupID = roomGroupID;
        this.numCells = numCells;
        this.pollution = pollution;
    }

    public static CachedPollutionInfo NewCachedPollutionInfo()
    {
        CachedPollutionInfo result = default;
        result.Reset();
        return result;
    }

    public void Reset()
    {
        roomGroupID = -1;
        numCells = 0;
        pollution = 0;
    }
}

public class PollutionCache : IExposable
{
    private readonly Map map;
    private readonly HashSet<int> processedRoomGroupIDs = new();
    private readonly List<CachedPollutionInfo> relevantPollutionInfos = new();
    public CachedPollutionInfo[] pollutionCache;
    internal PollutionSaveLoad pollutionSaveLoad;

    public PollutionCache(Map map)
    {
        this.map = map;
        pollutionCache = new CachedPollutionInfo[map.cellIndices.NumGridCells];
        pollutionSaveLoad = new PollutionSaveLoad(map);
    }

    public TiberiumPollutionMapInfo Pollution => map.Tiberium().PollutionInfo;

    public void ExposeData()
    {
        pollutionSaveLoad.DoExposing();
    }

    private void SetCachedInfo(IntVec3 c, CachedPollutionInfo pollution)
    {
        pollutionCache[map.cellIndices.CellToIndex(c)] = pollution;
    }

    public void ResetInfo(IntVec3 c)
    {
        pollutionCache[map.cellIndices.CellToIndex(c)].Reset();
    }

    public void TryCacheRegionPollutionInfo(IntVec3 c, Region reg)
    {
        var room = reg.Room;
        if (room == null) return;
        RoomGroup group = room.Group;
        SetCachedInfo(c, new CachedPollutionInfo(group.ID, group.CellCount, Pollution.TrackerFor(room).Pollution));
    }

    public bool TryGetAverageRoomPollution(RoomGroup r, out int result)
    {
        var cellIndices = map.cellIndices;
        foreach (var c in r.Cells)
        {
            var cachedInfo = pollutionCache[cellIndices.CellToIndex(c)];
            if (cachedInfo.numCells > 0 && !processedRoomGroupIDs.Contains(cachedInfo.roomGroupID))
            {
                relevantPollutionInfos.Add(cachedInfo);
                processedRoomGroupIDs.Add(cachedInfo.roomGroupID);
            }
        }

        var num = 0;
        var num2 = 0;
        foreach (var pollutionInfo in relevantPollutionInfos)
        {
            num++;
            var value = pollutionInfo.pollution;
            if (r.CellCount < pollutionInfo.numCells)
                value = Mathf.RoundToInt(value * (r.CellCount / ((float)pollutionInfo.numCells - 1)));
            num2 += value;
        }

        result = num2;
        var result2 = !relevantPollutionInfos.NullOrEmpty();
        processedRoomGroupIDs.Clear();
        relevantPollutionInfos.Clear();
        return result2;
    }
}

public class PollutionSaveLoad
{
    private readonly Map map;
    private ushort[] pollGrid;

    public PollutionSaveLoad(Map map)
    {
        this.map = map;
    }

    private TiberiumPollutionMapInfo PollutionMapInfo => map.Tiberium().PollutionInfo;

    public void ApplyLoadedDataToRegions()
    {
        if (pollGrid != null)
        {
            var cellIndices = map.cellIndices;
            foreach (var region in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
                if (region.Room != null)
                {
                    //TODO: Short to int
                    //PollutionMapInfo.TrackerFor(region.Room).Pollution = ShortToFloat(pollGrid[cellIndices.CellToIndex(region.Cells.First())]);
                }

            pollGrid = null;
        }
    }

    public void DoExposing()
    {
        byte[] arr = null;
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            var num = Mathf.RoundToInt(PollutionMapInfo.OutsidePollution);
            var num2 = FloatToShort(num);
            var tempGrid = new ushort[map.cellIndices.NumGridCells];
            for (var i = 0; i < map.cellIndices.NumGridCells; i++) tempGrid[i] = num2;

            foreach (var region in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
                if (region.Room != null)
                {
                    var num3 = FloatToShort(region.Room.Temperature);
                    foreach (var c2 in region.Cells) tempGrid[map.cellIndices.CellToIndex(c2)] = num3;
                }

            arr = MapSerializeUtility.SerializeUshort(map, c => tempGrid[map.cellIndices.CellToIndex(c)]);
        }

        DataExposeUtility.ByteArray(ref arr, "pollution");

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            pollGrid = new ushort[map.cellIndices.NumGridCells];
            MapSerializeUtility.LoadUshort(arr, map,
                delegate(IntVec3 c, ushort val) { pollGrid[map.cellIndices.CellToIndex(c)] = val; });
        }
    }

    private ushort FloatToShort(float value)
    {
        return (ushort)((int)(value * 16) + 32768);
    }

    private float ShortToFloat(ushort temp)
    {
        return (temp - 32768f) / 16f;
    }
}

public class PollutionRoomUpdater
{
    private readonly List<PollutionTracker> newTrackers = new();
    private readonly TiberiumPollutionMapInfo parentInfo;
    private List<PollutionTracker> existingTrackers = new();
    public List<RoomGroup> newRoomGroups = new();

    public List<RoomGroup> reusedRoomGroups = new();

    public bool updating;

    public PollutionRoomUpdater(TiberiumPollutionMapInfo mapInfo)
    {
        parentInfo = mapInfo;
    }

    public void Notify_UpdateStart()
    {
        existingTrackers = parentInfo.PollutionTrackers.ListFullCopy();
        parentInfo.PollutionTrackers.Clear();

        reusedRoomGroups.Clear();
        newRoomGroups.Clear();
        updating = true;
    }

    public void Notify_UpdateRoomGroups(List<RoomGroup> newRooms, HashSet<RoomGroup> reusedGroup)
    {
        newRoomGroups = newRooms.ListFullCopy();
        reusedRoomGroups = Enumerable.ToList(reusedGroup);
    }

    public void Apply(List<Room> newRooms)
    {
        foreach (var newRoom in newRooms)
        {
            if (newTrackers.Any(t => t.Group == newRoom.Group)) continue;
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
                if (newRoom.Group == reusedRoom)
                {
                    var tracker2 = existingTrackers.Find(t => t.Group == reusedRoom);
                    Log.Message("Updating reused room with existing tracker " + (tracker2 != null));
                    newTrackers.Add(tracker2);
                    break;
                }

            foreach (var newGroup in newRoomGroups)
                if (newRoom.Group == newGroup)
                {
                    var newTracker = new PollutionTracker(newGroup.Map, newGroup, 0);
                    newTracker.MarkDirty();
                    newTrackers.Add(newTracker);
                    //TODO: Check outdoor effect
                    if (newTracker.Group.UsesOutdoorTemperature) break;
                    if (parentInfo.pollutionCache.TryGetAverageRoomPollution(newGroup, out var pollution))
                        newTracker.Pollution = pollution;
                    break;
                }
        }

        parentInfo.PollutionTrackers.AddRange(newTrackers);
        newTrackers.Clear();

        foreach (var tracker in parentInfo.PollutionTrackers)
            if (tracker.IsDirty)
                tracker.RegenerateData();

        updating = false;
    }
}

/* OLD OLD REF
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumPollutionMapInfo : MapInformation
    {
        public static int CELL_CAPACITY = 100;

        private int lastPollutionInt;

        public PollutionCache Cache;
        public readonly PollutionContainer OutsideContainer;

        public readonly Dictionary<RoomGroup, RoomComponent_Pollution> PollutionComps = new Dictionary<RoomGroup, RoomComponent_Pollution>();

        public readonly List<RoomComponent_Pollution> AllComps = new List<RoomComponent_Pollution>();
        public readonly List<IPollutionSource> Sources = new List<IPollutionSource>();

        public readonly List<PollutionConnector> AllConnections = new List<PollutionConnector>();
        public readonly List<PollutionConnector> ConnectionsToOutside = new List<PollutionConnector>();

        //public int TotalMapPollution => OutsideContainer.Pollution + AllComps.Sum(c => c.PollutionContainer.Pollution);

        public int Pollution
        {
            get => OutsideContainer.Pollution;
            set => OutsideContainer.Pollution = value;
        }

        public TiberiumPollutionMapInfo(Map map) : base(map)
        {
            Cache = new PollutionCache(map);
            OutsideContainer = new PollutionContainer();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Cache, "Cache", map);
        }

        public RoomComponent_Pollution TrackerAt(IntVec3 pos)
        {
            return PollutionComps[pos.GetRoomGroup(map)];
        }

        public RoomComponent_Pollution PollutionFor(RoomGroup group)
        {
            return PollutionComps[group];
        }

        public RoomComponent_Pollution PollutionFor(Room room)
        {
            return PollutionComps[room.Group];
        }

        public void RegenerateOutside()
        {
            OutsideContainer.RegenerateData(AllComps.Where(c => c.UsesOutDoorPollution).Sum(c => c.Group.CellCount));
        }

        public override void Tick()
        {
            if (Find.TickManager.TicksGame % 10 == 0)
            {
                foreach (var pollutionComp in AllComps)
                {
                    pollutionComp.Equalize();
                }

                foreach (var connector in AllConnections)
                {
                    connector.TryEqualize();
                }
            }

            foreach (var source in Sources)
            {
                if (source?.Thing?.IsHashIntervalTick(source.PollutionInterval) ?? false)
                {
                    TryPollute(source);
                }
            }
        }

        private void TryPollute(IPollutionSource source)
        {
            if (!source.IsPolluting) return;
            if (PollutionComps[source.Room.Group].TryPollute(source.PollutionAmount))
            {
                //TODO: effect on source...
            }

            if (Pollution != lastPollutionInt)
            {
                GameCondition_TiberiumBiome mainCondition = (GameCondition_TiberiumBiome)map.GameConditionManager.GetActiveCondition(TiberiumDefOf.TiberiumBiome);
                if (mainCondition == null)
                {
                    GameCondition condition = GameConditionMaker.MakeCondition(TiberiumDefOf.TiberiumBiome);
                    condition.conditionCauser = TRUtils.Tiberium().GroundZeroInfo.GroundZero;
                    condition.Permanent = true;
                    mainCondition = (GameCondition_TiberiumBiome)condition;
                    map.GameConditionManager.RegisterCondition(condition);
                    Log.Message("Adding game condition..");
                }

                if (!mainCondition.AffectedMaps.Contains(this.map))
                {
                    mainCondition.AffectedMaps.Add(map);
                    Log.Message("Adding map to game condition..");
                }
                //mainCondition.Notify_PollutionChange(map, OutsideContainer.Saturation);
            }

            lastPollutionInt = Pollution;
        }

        public void RegisterSource(IPollutionSource source)
        {
            if (Sources.Contains(source)) return;
            Sources.Add(source);
        }

        public void DeregisterSource(IPollutionSource source)
        {
            Sources.Remove(source);
        }

        public void Notify_AddConnection(PollutionConnector connection)
        {
            var same = AllConnections.Find(t => t.IsSameBuilding(connection));
            Log.Message("Adding... " + connection.Building + " ConnectsOutside? " + connection.ConnectsOutside() + " Already Exists? " + (same != null));
            if (same != null)
            {
                Log.Message("Replacing... " + same.Building + " ConnectsOutside? " + same.ConnectsOutside());
                Notify_RemoveConnection(same);
            }

            if (connection.ConnectsOutside())
            {
                ConnectionsToOutside.Add(connection);
            }

            AllConnections.Add(connection);
        }

        public void Notify_RemoveConnection(PollutionConnector connection)
        {
            Log.Message("Removing... " + connection.Building);
            ConnectionsToOutside.RemoveAll(c => c.IsSameBuilding(connection));
            AllConnections.RemoveAll(c => c.IsSameBuilding(connection));
        }

        public void Notify_NewComp(RoomComponent_Pollution comp)
        {
            AllComps.Add(comp);
            PollutionComps.Add(comp.Group, comp);
        }

        public void Notify_DisbandedComp(RoomComponent_Pollution comp)
        {
            AllComps.Remove(comp);
            PollutionComps.Remove(comp.Group);
        }
    }
}
*/