using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PollutionPasser
    {
        private Building building;

        public Building Building => building;

        public PollutionPasser(Building b)
        {
            building = b;
        }

        public bool CanPass
        {
            get
            {
                return building switch
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
        public PollutionRoomUpdater updater;
        public PollutionCache pollutionCache;
        public List<PollutionTracker> PollutionTrackers = new List<PollutionTracker>();

        public List<IPollutionSource> Sources = new List<IPollutionSource>();

        private int outsidePollInt = 0;
        public int OutsidePollution
        {
            get => outsidePollInt;
            set => outsidePollInt = value;
        }

        public TiberiumPollutionMapInfo(Map map) : base(map)
        {
            updater = new PollutionRoomUpdater(this);
            pollutionCache = new PollutionCache(map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
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

        [TweakValue("Pollution_Ticks", 1f, 250f)]
        public static int Ticks = 100;

        public override void Tick()
        {
            if (Find.TickManager.TicksGame % Ticks == 10)
            {
                foreach (var tracker in PollutionTrackers)
                {
                    tracker.Equalize();
                }
            }

            foreach (var source in Sources)
            {
                if (source?.Thing?.IsHashIntervalTick(source.PollutionInterval) ?? false)
                {
                    Pollute(source);
                }
            }
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
            foreach (var tracker in PollutionTrackers)
            {
                tracker.OnGUI();
            }
        }

        public override void Draw()
        {
            if (!TRUtils.Tiberium().GameSettings.RadiationOverlay) return;
            foreach (var pollutionTracker in PollutionTrackers)
            {
                /*
                foreach (var cell in pollutionTracker.Group.Cells)
                {
                    CellRenderer.RenderCell(cell, pollutionTracker.Saturation);
                }*/
                pollutionTracker.DrawData();
            }
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
            this.roomGroupID = -1;
            this.numCells = 0;
            this.pollution = 0;
        }
    }

    public class PollutionCache : IExposable
    {
        private Map map;
        private HashSet<int> processedRoomGroupIDs = new HashSet<int>();
        private List<CachedPollutionInfo> relevantPollutionInfos = new List<CachedPollutionInfo>();
        public CachedPollutionInfo[] pollutionCache;
        internal PollutionSaveLoad pollutionSaveLoad;

        public TiberiumPollutionMapInfo Pollution => map.Tiberium().PollutionInfo;

        public PollutionCache(Map map)
        {
            this.map = map;
            this.pollutionCache = new CachedPollutionInfo[map.cellIndices.NumGridCells];
            this.pollutionSaveLoad = new PollutionSaveLoad(map);
        }

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
            Room room = reg.Room;
            if (room == null) return;
            RoomGroup group = room.Group;
            SetCachedInfo(c, new CachedPollutionInfo(group.ID, group.CellCount, Pollution.TrackerFor(room).Pollution));
        }

        public bool TryGetAverageRoomPollution(RoomGroup r, out int result)
        {
            CellIndices cellIndices = this.map.cellIndices;
            foreach (var c in r.Cells)
            {
                CachedPollutionInfo cachedInfo = this.pollutionCache[cellIndices.CellToIndex(c)];
                if (cachedInfo.numCells > 0 && !processedRoomGroupIDs.Contains(cachedInfo.roomGroupID))
                {
                    relevantPollutionInfos.Add(cachedInfo);
                    processedRoomGroupIDs.Add(cachedInfo.roomGroupID);
                }
            }

            int num = 0;
            int num2 = 0;
            foreach (var pollutionInfo in relevantPollutionInfos)
            {
                num++;
                var value = pollutionInfo.pollution;
                if (r.CellCount < pollutionInfo.numCells)
                {
                    value = Mathf.RoundToInt(value * (r.CellCount / ((float) pollutionInfo.numCells - 1)));
                }
                num2 += value;
            }

            result = num2;
            bool result2 = !relevantPollutionInfos.NullOrEmpty();
            processedRoomGroupIDs.Clear();
            relevantPollutionInfos.Clear();
            return result2;
        }
    }

    public class PollutionSaveLoad
    {
        private Map map;
        private ushort[] pollGrid;

        private TiberiumPollutionMapInfo PollutionMapInfo => map.Tiberium().PollutionInfo;

        public PollutionSaveLoad(Map map)
        {
            this.map = map;
        }

        public void ApplyLoadedDataToRegions()
        {
            if (pollGrid != null)
            {
                CellIndices cellIndices = map.cellIndices;
                foreach (Region region in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
                {
                    if (region.Room != null)
                    {
                        //TODO: Short to int
                        //PollutionMapInfo.TrackerFor(region.Room).Pollution = ShortToFloat(pollGrid[cellIndices.CellToIndex(region.Cells.First())]);
                    }
                }
                pollGrid = null;
            }
        }

        public void DoExposing()
        {
            byte[] arr = null;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                int num = Mathf.RoundToInt(PollutionMapInfo.OutsidePollution);
                ushort num2 = FloatToShort(num);
                ushort[] tempGrid = new ushort[map.cellIndices.NumGridCells];
                for (int i = 0; i < map.cellIndices.NumGridCells; i++)
                {
                    tempGrid[i] = num2;
                }

                foreach (Region region in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
                {
                    if (region.Room != null)
                    {
                        ushort num3 = FloatToShort(region.Room.Temperature);
                        foreach (IntVec3 c2 in region.Cells)
                        {
                            tempGrid[map.cellIndices.CellToIndex(c2)] = num3;
                        }
                    }
                }

                arr = MapSerializeUtility.SerializeUshort(map, c => tempGrid[map.cellIndices.CellToIndex(c)]);
            }

            DataExposeUtility.ByteArray(ref arr, "pollution");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                pollGrid = new ushort[map.cellIndices.NumGridCells];
                MapSerializeUtility.LoadUshort(arr, map, delegate(IntVec3 c, ushort val) { pollGrid[map.cellIndices.CellToIndex(c)] = val; });
            }
        }

        private ushort FloatToShort(float value)
        {
            return (ushort) ((int) (value * 16) + 32768);
        }

        private float ShortToFloat(ushort temp)
        {
            return ((float)temp - 32768f) / 16f;
        }
    }

    public class PollutionRoomUpdater
    {
        private TiberiumPollutionMapInfo parentInfo;
        private List<PollutionTracker> existingTrackers = new List<PollutionTracker>();
        private List<PollutionTracker> newTrackers = new List<PollutionTracker>();

        public List<RoomGroup> reusedRoomGroups = new List<RoomGroup>();
        public List<RoomGroup> newRoomGroups = new List<RoomGroup>();

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
            reusedRoomGroups = reusedGroup.ToList();
        }

        public void Apply(List<Room> newRooms)
        {
            foreach (var newRoom in newRooms)
            {
                if(newTrackers.Any(t => t.Group == newRoom.Group)) continue;
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
                {
                    if (newRoom.Group == reusedRoom)
                    {
                        var tracker2 = existingTrackers.Find(t => t.Group == reusedRoom);
                        Log.Message("Updating reused room with existing tracker " + (tracker2 != null));
                        newTrackers.Add(tracker2);
                        break;
                    }
                }

                foreach (var newGroup in newRoomGroups)
                {
                    if (newRoom.Group == newGroup)
                    {
                        var newTracker = new PollutionTracker(newGroup.Map, newGroup, 0);
                        newTracker.MarkDirty();
                        newTrackers.Add(newTracker);
                        //TODO: Check outdoor effect
                        if(newTracker.Group.UsesOutdoorTemperature) break;
                        if (parentInfo.pollutionCache.TryGetAverageRoomPollution(newGroup, out int pollution))
                        {
                            newTracker.Pollution = pollution;
                        }
                        break;
                    }
                }
            }

            parentInfo.PollutionTrackers.AddRange(newTrackers);
            newTrackers.Clear();

            foreach (var tracker in parentInfo.PollutionTrackers)
            {
                if(tracker.IsDirty) tracker.RegenerateData();
            }

            updating = false;
        }
    }
}
