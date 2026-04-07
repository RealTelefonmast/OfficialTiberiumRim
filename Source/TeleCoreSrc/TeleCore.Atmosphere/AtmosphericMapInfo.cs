using System.Collections.Generic;
using RimWorld;
using TeleCore.Atmosphere.Caching;
using TeleCore.Atmosphere.Defs;
using TeleCore.Atmosphere.Grid;
using TeleCore.Atmosphere.Rendering;
using TeleCore.Atmosphere.Rooms;
using TeleCore.Atmosphere.Rooms.Converters;
using TeleCore.Events;
using TeleCore.Logging;
using TeleCore.MapInfo;
using TeleCore.Primitive;
using TeleCore.Utility;
using TeleCore.Utils;
using Verse;

namespace TeleCore.Atmosphere;

public class AtmosphericMapInfo : MapInformation
{
    //Scribing
    private AtmosphericScriber _scriber;
    private AtmosphericCache _cache;

    //System for room-nased atmospheric flow
    private AtmosphericSystem _system;
    private AtmosphericMapConverters _converters;

    private readonly AtmosphereRenderer _renderer;
    private readonly Dictionary<Room, RoomComponent_Atmosphere> _compLookUp;
    private readonly List<RoomComponent_Atmosphere> _allComps;

    //
    public List<RoomComponent_Atmosphere> AllAtmosphericRooms => _allComps;
    public AtmosphereRenderer Renderer => _renderer;
    public AtmosphericSystem System => _system;
    public AtmosphericMapConverters Converters => _converters;
    public AtmosphericVolume MapVolume => _system.MapVolume;
    internal AtmosphericCache Cache => _cache;

    public AtmosphericMapInfo(Map map) : base(map)
    {
        _scriber = new AtmosphericScriber(this);
        _cache = new AtmosphericCache(map);
        _system = new AtmosphericSystem(map);
        _converters = new AtmosphericMapConverters();

        //
        _compLookUp = new Dictionary<Room, RoomComponent_Atmosphere>();
        _allComps = new List<RoomComponent_Atmosphere>();

        //
        _renderer = new AtmosphereRenderer(map);
    }

    public override void ExposeDataExtra()
    {
        Scribe_Deep.Look(ref _scriber, "cacheData", this);
    }

    //
    public RoomComponent_Atmosphere ComponentAt(IntVec3 pos)
    {
        var room = pos.GetRoomFast(Map);
        return ComponentAt(room);
    }

    public RoomComponent_Atmosphere ComponentAt(District district)
    {
        if (district is null) return null;
        return ComponentAt(district.Room);
    }

    public RoomComponent_Atmosphere ComponentAt(Room room)
    {
        if (room is null) return null;
        if (!_compLookUp.TryGetValue(room, out var value))
        {
            Log.Warning($"Could not find RoomComponent_Atmospheric at room {room.ID}");
            return null;
        }
        return value;
    }

    public override void InfoInit(bool initAfterReload = false)
    {
        base.InfoInit(initAfterReload);

        RegenerateMapInfo();
        _system.Init(map);
        //map.GameConditionManager.RegisterCondition(GameConditionMaker.MakeConditionPermanent(AtmosDefOf.AtmosphericCondition));
    }

    public void RegenerateMapInfo()
    {
        TLog.Message("Regenerating map info...");
        _system.Notify_Regenerate(Map.cellIndices.NumGridCells); //AllComps.Where(c => c.IsOutdoors).Sum(c => c.Room.CellCount)
    }

    //
    public override void Tick()
    {
        var tick = Find.TickManager.TicksGame;

        _system.Tick(tick);
        _converters.Tick();
        _renderer.Tick();
    }

    #region Data

    public void Notify_UpdateRoomComp(RoomComponent_Atmosphere comp)
    {
        _system.Notify_UpdateRoomComp(comp);
    }

    public void Notify_AddRoomComp(RoomComponent_Atmosphere comp)
    {
        if (_compLookUp.TryAdd(comp.Room, comp))
        {
            _allComps.Add(comp);
            _system.Notify_AddRoomComp(comp);
        }
        else
        {
            TLog.Warning($"Tried to add existin roomComp: {comp.Room.ID} | IsDisbanded? {comp.Disbanded}");
        }
    }

    public void Notify_RemoveRoomComp(RoomComponent_Atmosphere comp)
    {
        _allComps.Remove(comp);
        _compLookUp.Remove(comp.Room);
        _system.Notify_RemoveRoomComp(comp);
    }

    //Things
    public void Notify_ThingSentSignal(ThingStateChangedEventArgs args)
    {
        switch (args.Thing)
        {
            case Building_Vent or Building_Cooler:
                var rot = args.Thing.Rotation;
                var positionA = args.Thing.Position + rot.FacingCell;
                var positionB = args.Thing.Position + rot.Opposite.FacingCell;
                var roomA = positionA.GetRoomFast(Map);
                var roomB = positionB.GetRoomFast(Map);

                var infront = _compLookUp.TryGetValue(roomA);
                var behind = _compLookUp.TryGetValue(roomB);
                System.Notify_InterfaceBetweenRoomsChanged(infront, behind, args.Thing, args.CompSignal);
                break;
            case Building_Door door:
                var tracker = _compLookUp.TryGetValue(door.GetRoom());
                foreach (var neighbor in tracker.CompNeighbors.Neighbors)
                {
                    System.Notify_InterfaceBetweenRoomsChanged(tracker, neighbor, door, args.CompSignal);
                }
                break;
        }
    }

    //Atmospher Scribing
    public void Notify_LoadedOutsideAtmosphere(DefValueStack<AtmosphericValueDef,double> stack)
    {

    }

    public void Notify_ApplyLoadedData()
    {
        _scriber.ApplyLoadedDataToRegions();
    }

    public void TrySpawnGasAt(IntVec3 cell, SpreadingGasTypeDef gasType, float value)
    {
        Map.GetMapInfo<SpreadingGasGrid>().Notify_SpawnGasAt(cell, gasType, value);
    }

    #endregion

    #region Rendering

    public override void UpdateOnGUI()
    {
    }

    public override void Update()
    {
        base.Update();
        _renderer.AtmosphereDrawerUpdate();
        _renderer.Draw();
    }

    #endregion
}
