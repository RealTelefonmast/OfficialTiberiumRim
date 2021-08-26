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

        public readonly Dictionary<Room, RoomComponent_Pollution> PollutionComps = new Dictionary<Room, RoomComponent_Pollution>();

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
            return PollutionComps[pos.GetRoom(map)];
        }

        public RoomComponent_Pollution PollutionFor(District district)
        {
            return PollutionComps[district.Room];
        }


        public RoomComponent_Pollution PollutionFor(Room room)
        {
            return PollutionComps[room];
        }

        public void RegenerateOutside()
        {
            OutsideContainer.RegenerateData(AllComps.Where(c => c.UsesOutDoorPollution).Sum(c => c.Room.CellCount));
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
            if (PollutionComps[source.Room].TryAddPollution(source.PollutionAmount, out _))
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

        /*
        public void Notify_AddConnection(PollutionConnector connection)
        {
            //Log.Message("Adding connection " + connection);
            //Check for duplicate
            var same = ConnectionToOutside.Find(t => t.IsSameBuilding(connection));
            if (same != null)
            {
                ConnectionToOutside.Remove(same);
                ConnectionToOutside.Add(connection);
                return;
            }
            ConnectionToOutside.Add(connection);
        }

        public void Notify_RemoveConnection(PollutionConnector connection)
        {
            if(ConnectionToOutside.Contains(connection))
                ConnectionToOutside.Remove(connection);
        }

        public void Notify_ValidateConnections()
        {
            for (var i = ConnectionToOutside.Count - 1; i >= 0; i--)
            {
                var globalConnection = ConnectionToOutside[i];
                if (globalConnection.Building.Destroyed || globalConnection.IsOutside())
                {
                    ConnectionToOutside.Remove(globalConnection);
                }
            }
        }
        */

        public void Notify_NewComp(RoomComponent_Pollution comp)
        {
            AllComps.Add(comp);
            PollutionComps.Add(comp.Room, comp);
        }

        public void Notify_DisbandedComp(RoomComponent_Pollution comp)
        {
            AllComps.Remove(comp);
            PollutionComps.Remove(comp.Room);
        }
    }

    public struct CachedPollutionInfo
    {
        public int roomID;
        public int numCells;
        public int pollution;

        public CachedPollutionInfo(int roomID, int numCells, int pollution)
        {
            this.roomID = roomID;
            this.numCells = numCells;
            this.pollution = pollution;
        }

        public void Reset()
        {
            this.roomID = -1;
            this.numCells = 0;
            this.pollution = 0;
        }
    }

    public class PollutionCache : IExposable
    {
        private Map map;
        private HashSet<int> processedRoomIDs = new HashSet<int>();
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
            this.pollutionSaveLoad.DoExposing();
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
            var poll = Pollution.PollutionFor(room).Pollution;
            SetCachedInfo(c, new CachedPollutionInfo(room.ID, room.CellCount, poll));
        }

        public bool TryGetAverageRoomPollution(Room r, out int result)
        {
            CellIndices cellIndices = this.map.cellIndices;
            foreach (var c in r.Cells)
            {
                CachedPollutionInfo cachedInfo = this.pollutionCache[cellIndices.CellToIndex(c)];
                if (cachedInfo.numCells > 0 && !processedRoomIDs.Contains(cachedInfo.roomID))
                {
                    relevantPollutionInfos.Add(cachedInfo);
                    processedRoomIDs.Add(cachedInfo.roomID);
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
            processedRoomIDs.Clear();
            relevantPollutionInfos.Clear();
            return result2;
        }
    }

    public class PollutionSaveLoad
    {
        private Map map;
        private int[] pollGrid;

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
                PollutionMapInfo.Pollution = pollGrid[map.cellIndices.NumGridCells]; //ShortToInt(pollGrid[map.cellIndices.NumGridCells]);
                Log.Message("Applying Outside Pollution: " + PollutionMapInfo.Pollution);

                foreach (var tracker in PollutionMapInfo.PollutionComps)
                {
                    int val = pollGrid[cellIndices.CellToIndex(tracker.Key.Cells.First())];
                    tracker.Value.Pollution = val;
                    Log.Message("Applying on Tracker " + tracker.Key.ID + " with " + val);
                }
                //
                foreach (Region region in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
                {
                    if (region.Room != null)
                    {
                        int val = pollGrid[cellIndices.CellToIndex(region.Cells.First())];
                        Log.Message("Applying on Region " + region.Room.ID + " with " + val);
                        PollutionMapInfo.PollutionFor(region.Room).Pollution = val;
                    }
                }
                //

                pollGrid = null;
            }
        }
        
        public void DoExposing()
        {
            Log.Message("Exposing Pollution");
            byte[] arr = null;
            int arraySize = map.cellIndices.NumGridCells + 1;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                int[] tempGrid = new int[arraySize];
                int num = Mathf.RoundToInt(PollutionMapInfo.Pollution);

                tempGrid[arraySize - 1] = num;
                Log.Message("Saving Outside Pollution: " + PollutionMapInfo.Pollution + " as " + num);

                foreach (var roomComp in PollutionMapInfo.AllComps)
                {
                    if (roomComp.UsesOutDoorPollution) continue;
                    int num3 = roomComp.ActualPollution;
                    Log.Message("Saving RoomComp " + roomComp.Room.ID + " with " + roomComp.ActualPollution + " as " + num3);
                    foreach (IntVec3 c2 in roomComp.Room.Cells)
                    {
                        tempGrid[map.cellIndices.CellToIndex(c2)] = num3;
                    }
                }

                arr = DataSerializeUtility.SerializeInt(arraySize, (int idx) => tempGrid[idx]);
            }

            DataExposeUtility.ByteArray(ref arr, "tiberiumPollution");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                pollGrid = new int[arraySize];
                DataSerializeUtility.LoadInt(arr, arraySize, delegate(int idx, int data) { pollGrid[idx] = data; });
            }
        }
    }
}


/*
    public class TiberiumPollutionMapInfo2 : MapInformation
    {
    public PollutionRoomUpdater updater;
    public PollutionCache pollutionCache;
    public Dictionary<RoomGroup, PollutionTracker> PollutionTrackers = new Dictionary<RoomGroup, PollutionTracker>();
    private OutsidePollutionData outsideData;

    public List<IPollutionSource> Sources = new List<IPollutionSource>();

    public int TotalPollution => OutsideData.Pollution + PollutionTrackers.Sum(t => t.Value.ActualPollution);

    public OutsidePollutionData OutsideData => outsideData;

    public TiberiumPollutionMapInfo2(Map map) : base(map)
    {
    updater = new PollutionRoomUpdater(this);
    pollutionCache = new PollutionCache(map);
    outsideData = new OutsidePollutionData();
    }

    public override void ExposeData()
    {
    base.ExposeData();
    Scribe_Deep.Look(ref pollutionCache, "pollutionCache", map);
    }

    public PollutionTracker PollutionAt(IntVec3 pos)
    {
    return PollutionTrackers[pos.GetRoomGroup(map)];
    }

    public PollutionTracker PollutionFor(RoomGroup group)
    {
    return PollutionTrackers[group];
    }

    public PollutionTracker PollutionFor(Room room)
    {
    return PollutionTrackers[room.Group];
    }

    public void RecalcOutsideCells()
    {
    outsideData.RegenerateData(PollutionTrackers.Where(p => p.Key.UsesOutdoorTemperature).Sum(p => p.Key.CellCount));
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
    tracker.Value.Equalize();
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
    if (PollutionFor(source.Room).TryPollute(source.PollutionAmount))
    {
    //TODO: effect on source...
    }
    }

    public override void UpdateOnGUI()
    {
    foreach (var tracker in PollutionTrackers)
    {
    tracker.Value.OnGUI();
    }
    }

    public override void Draw()
    {
    foreach (var pollutionTracker in PollutionTrackers)
    {
    pollutionTracker.Value.DrawData();
    }
    }
    }
*/
