using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AtmosphericMapInfo : MapInformation
    {
        public static int CELL_CAPACITY = 100;

        public AtmosphericCache Cache;
        public readonly AtmosphericContainer OutsideContainer;

        public readonly Dictionary<Room, RoomComponent_Atmospheric> PollutionComps = new Dictionary<Room, RoomComponent_Atmospheric>();

        public readonly List<RoomComponent_Atmospheric> AllComps = new List<RoomComponent_Atmospheric>();
        public readonly List<IAtmosphericSource> Sources = new List<IAtmosphericSource>();

        public readonly List<AtmosphericConnector> AllConnections = new List<AtmosphericConnector>();
        public readonly List<AtmosphericConnector> ConnectionsToOutside = new List<AtmosphericConnector>();

        //public int TotalMapPollution => OutsideContainer.Atmospheric + AllComps.Sum(c => c.PollutionContainer.Atmospheric);

        public int TotalAtmosphericValue
        {
            get => OutsideContainer.Value;
            //set => OutsideContainer.Pollution = value;
        }

        public AtmosphericMapInfo(Map map) : base(map)
        {
            Cache = new AtmosphericCache(map);
            OutsideContainer = new AtmosphericContainer(null);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Cache, "Cache", map);
        }

        public RoomComponent_Atmospheric ComponentAt(IntVec3 pos)
        {
            return PollutionComps[pos.GetRoom(map)];
        }

        public RoomComponent_Atmospheric ComponentAt(District district)
        {
            return PollutionComps[district.Room];
        }


        public RoomComponent_Atmospheric ComponentAt(Room room)
        {
            return PollutionComps[room];
        }

        public void RegenerateOutside()
        {
            var totalCells = Map.cellIndices.NumGridCells; //AllComps.Where(c => c.IsOutdoors).Sum(c => c.Room.CellCount) 
            OutsideContainer.RegenerateData(null, totalCells);
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
                if (source?.Thing?.IsHashIntervalTick(source.CreationInterval) ?? false)
                {
                    //TryPollute(source);
                }
            }
        }

        private void TryPollute(IAtmosphericSource source)
        {
            if (!source.IsActive) return;
            if (PollutionComps[source.Room].TryAddValue(source.AtmosphericType, source.CreationAmount, out _))
            {
                //TODO: effect on source...
            }

            /*
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
            */
        }

        public void RegisterSource(IAtmosphericSource source)
        {
            if (Sources.Contains(source)) return;
            Sources.Add(source);
        }

        public void DeregisterSource(IAtmosphericSource source)
        {
            Sources.Remove(source);
        }

        public void Notify_AddConnection(AtmosphericConnector connection)
        {
            var same = AllConnections.Find(t => t.IsSameBuilding(connection));
            if (same != null)
            {
                Notify_RemoveConnection(same);
            }

            if (connection.ConnectsOutside())
            {
                ConnectionsToOutside.Add(connection);
            }

            AllConnections.Add(connection);
        }

        public void Notify_RemoveConnection(AtmosphericConnector connection)
        {
            ConnectionsToOutside.RemoveAll(c => c.IsSameBuilding(connection));
            AllConnections.RemoveAll(c => c.IsSameBuilding(connection));
        }

        /*
        public void Notify_AddConnection(AtmosphericConnector connection)
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

        public void Notify_RemoveConnection(AtmosphericConnector connection)
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

        public void Notify_NewComp(RoomComponent_Atmospheric comp)
        {
            AllComps.Add(comp);
            PollutionComps.Add(comp.Room, comp);
        }

        public void Notify_DisbandedComp(RoomComponent_Atmospheric comp)
        {
            AllComps.Remove(comp);
            PollutionComps.Remove(comp.Room);
        }

        public bool TrySpawnGasAt(IntVec3 cell, ThingDef def,  int value)
        {
            if (!ComponentAt(cell).CanHaveTangibleGas) return false;
            if (cell.GetGas(Map) is SpreadingGas existingGas)
            {
                existingGas.AdjustSaturation(value, out _);
                return true;
            }
            ((SpreadingGas)GenSpawn.Spawn(def, cell, Map)).AdjustSaturation(value, out _);
            return true;
        }
    }

    public struct CachedAtmosphereInfo : IExposable
    {
        public int roomID;
        public int numCells;
        public NetworkValueStack stack;

        public CachedAtmosphereInfo(int roomID, int numCells, NetworkValueStack stack)
        {
            this.roomID = roomID;
            this.numCells = numCells;
            this.stack = stack;
        }

        public void Reset()
        {
            this.roomID = -1;
            this.numCells = 0;
            stack.Reset();
        }

        public void ExposeData()
        {

        }
    }

    public class AtmosphericCache : IExposable
    {
        private Map map;
        private HashSet<int> processedRoomIDs = new HashSet<int>();
        public CachedAtmosphereInfo[] pollutionCache;
        internal AtmosphericSaveLoad atmosphericSaveLoad;

        public AtmosphericMapInfo AtmosphericInfo => map.Tiberium().AtmosphericInfo;

        public AtmosphericCache(Map map)
        {
            this.map = map;
            this.pollutionCache = new CachedAtmosphereInfo[map.cellIndices.NumGridCells];
            this.atmosphericSaveLoad = new AtmosphericSaveLoad(map);
        }

        public void ExposeData()
        {
            this.atmosphericSaveLoad.DoExposing();
        }

        private void SetCachedInfo(IntVec3 c, CachedAtmosphereInfo atmosphere)
        {
            //Log.Message($"Caching stack for {atmosphere.roomID}: {atmosphere.stack} [{atmosphere.numCells}]");
            pollutionCache[map.cellIndices.CellToIndex(c)] = atmosphere;
        }

        public void ResetInfo(IntVec3 c)
        {
            pollutionCache[map.cellIndices.CellToIndex(c)].Reset();
        }

        public void TryCacheRegionAtmosphericInfo(IntVec3 c, Region reg)
        {
            Room room = reg.Room;
            if (room == null) return;
            SetCachedInfo(c, new CachedAtmosphereInfo(room.ID, room.CellCount, AtmosphericInfo.ComponentAt(room).ActualContainer.ValueStack));
        }

        public bool TryGetAtmosphericValuesForRoom(Room r, out Dictionary<NetworkValueDef, int> result)
        {
            CellIndices cellIndices = this.map.cellIndices;
            result = new();
            foreach (var c in r.Cells)
            {
                CachedAtmosphereInfo cachedInfo = this.pollutionCache[cellIndices.CellToIndex(c)];
                //If already processed or not a room, ignore
                if (cachedInfo.numCells <= 0 || processedRoomIDs.Contains(cachedInfo.roomID) || cachedInfo.stack.Empty) continue;
                processedRoomIDs.Add(cachedInfo.roomID);
                foreach (var value in cachedInfo.stack.networkValues)
                {
                    if (!result.ContainsKey(value.valueDef))
                    {
                        result.Add(value.valueDef, 0);
                    }

                    float addedValue = value.value;
                    if (cachedInfo.numCells > r.CellCount)
                    {
                        addedValue = value.value * (r.CellCount / (float)cachedInfo.numCells);
                    }
                    //var adder = value.value * (Mathf.Min(cachedInfo.numCells, r.CellCount)/(float)Mathf.Max(cachedInfo.numCells , r.CellCount));
                    result[value.valueDef] += Mathf.CeilToInt(addedValue);
                }
            }

            processedRoomIDs.Clear();
            return !result.NullOrEmpty();
        }
    }

    public class AtmosphericSaveLoad
    {
        private Map map;

        private NetworkValueStack[] temporaryGrid;
        private NetworkValueStack[] atmosphericGrid;

        private AtmosphericMapInfo AtmosphericMapInfo => map.Tiberium().AtmosphericInfo;

        public AtmosphericSaveLoad(Map map)
        {
            this.map = map;
        }

        public void ApplyLoadedDataToRegions()
        {
            if (atmosphericGrid == null) return;

            CellIndices cellIndices = map.cellIndices;
            AtmosphericMapInfo.OutsideContainer.Container.LoadFromStack(atmosphericGrid[map.cellIndices.NumGridCells]); //ShortToInt(pollGrid[map.cellIndices.NumGridCells]);
            TLog.Debug($"Applying Outside Atmospheric: {atmosphericGrid[map.cellIndices.NumGridCells]}");

            foreach (var comp in AtmosphericMapInfo.PollutionComps)
            {
                var valueStack = atmosphericGrid[cellIndices.CellToIndex(comp.Key.Cells.First())];
                comp.Value.ActualContainer.Container.LoadFromStack(valueStack);
                //Log.Message($"Applying on Tracker {comp.Key.ID}: {valueStack}");
            }
            //
            /*
            foreach (Region region in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
            {
                if (region.Room != null)
                {
                    var valueStack = atmosphericGrid[cellIndices.CellToIndex(region.Cells.First())];
                    Log.Message($"Applying on Region  {region.Room.ID}: {valueStack}");
                    AtmosphericMapInfo.PollutionFor(region.Room).ActualContainer.Container.LoadFromStack(valueStack);
                }
            }
            */
            //

            atmosphericGrid = null;
        }
        
        public void DoExposing()
        {
            TLog.Debug("Exposing Atmospheric");
            int arraySize = map.cellIndices.NumGridCells + 1;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                temporaryGrid = new NetworkValueStack[arraySize];
                var outsideAtmosphere = AtmosphericMapInfo.OutsideContainer.Container.ValueStack;
                temporaryGrid[arraySize - 1] = outsideAtmosphere;

                foreach (var roomComp in AtmosphericMapInfo.AllComps)
                {
                    if (roomComp.IsOutdoors) continue;
                    var roomAtmosphereStack = roomComp.ActualContainer.Container.ValueStack;
                    foreach (IntVec3 c2 in roomComp.Room.Cells)
                    {
                        temporaryGrid[map.cellIndices.CellToIndex(c2)] = roomAtmosphereStack;
                    }
                }
            }

            //Turn temp grid into byte arrays
            var savableTypes = AtmosphericMapInfo.OutsideContainer.Container.AcceptedTypes;
            foreach (var type in savableTypes)
            {
                byte[] dataBytes = null;
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    dataBytes = DataSerializeUtility.SerializeInt(arraySize, (int idx) => temporaryGrid[idx].networkValues?.FirstOrFallback(f => f.valueDef == type).value ?? 0);
                    DataExposeUtility.ByteArray(ref dataBytes, $"{type.defName}.atmospheric");
                }

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    DataExposeUtility.ByteArray(ref dataBytes, $"{type.defName}.atmospheric");
                    atmosphericGrid = new NetworkValueStack[arraySize];
                    DataSerializeUtility.LoadInt(dataBytes, arraySize, delegate(int idx, int idxValue)
                    {
                        /*
                        if (idxValue > 0)
                        {
                            Log.Message($"Loading {idx}[{map.cellIndices.IndexToCell(idx)}]: {idxValue} ({type})");
                        }
                        */
                        atmosphericGrid[idx] += new NetworkValueStack(type, idxValue);
                    });
                }
            }
        }
    }
}


/*
    public class TiberiumPollutionMapInfo2 : MapInformation
    {
    public PollutionRoomUpdater updater;
    public AtmosphericCache pollutionCache;
    public Dictionary<RoomGroup, PollutionTracker> PollutionTrackers = new Dictionary<RoomGroup, PollutionTracker>();
    private OutsidePollutionData outsideData;

    public List<IAtmosphericSource> Sources = new List<IAtmosphericSource>();

    public int TotalPollution => OutsideData.Atmospheric + PollutionTrackers.Sum(t => t.Value.ActualPollution);

    public OutsidePollutionData OutsideData => outsideData;

    public TiberiumPollutionMapInfo2(Map map) : base(map)
    {
    updater = new PollutionRoomUpdater(this);
    pollutionCache = new AtmosphericCache(map);
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

    public void RegisterSource(IAtmosphericSource source)
    {
    if (Sources.Contains(source)) return;
    Sources.Add(source);
    }

    public void DeregisterSource(IAtmosphericSource source)
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
    tracker.Value.EqualizeWith();
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

    private void TryPollute(IAtmosphericSource source)
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
