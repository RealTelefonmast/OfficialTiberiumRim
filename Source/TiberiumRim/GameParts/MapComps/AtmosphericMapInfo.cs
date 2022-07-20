using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AtmosphericMapInfo : MapInformation
    {
        public static int CELL_CAPACITY = 100;

        public AtmosphericCache Cache;
        public readonly AtmosphericContainer OutsideContainer;

        //
        public readonly Dictionary<Room, RoomComponent_Atmospheric> PollutionComps = new();
        public readonly List<RoomComponent_Atmospheric> AllComps = new();

        //
        public readonly List<IAtmosphericSource> Sources = new();

        //
        public readonly List<AtmosphericConnector> AllConnections = new();
        public readonly List<AtmosphericConnector> ConnectionsToOutside = new();

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
            var room = pos.GetRoomFast(Map);
            return ComponentAt(room);
        }

        public RoomComponent_Atmospheric ComponentAt(District district)
        {
            if (district is null) return null;
            return ComponentAt(district.Room);
        }

        public RoomComponent_Atmospheric ComponentAt(Room room)
        {
            if (room is null) return null;
            if (!PollutionComps.TryGetValue(room, out var value))
            {
                TRLog.Warning($"Could not find RoomComponent_Atmospheric at room {room.ID}");
                return null;
            }
            return value;
        }

        //
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

    public struct CachedAtmosphereInfo
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
