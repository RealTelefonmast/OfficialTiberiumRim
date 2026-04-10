using System.Collections.Generic;
using RimWorld;
using TeleCore.Atmosphere.Caching;
using TeleCore.Atmosphere.Defs;
using TeleCore.Atmosphere.Grid;
using TeleCore.Atmosphere.Rendering;
using TeleCore.Atmosphere.Rooms;
using TeleCore.Atmosphere.Rooms.Converters;
using TeleCore.Events;
using TeleCore.Events.Args;
using TeleCore.GameData;
using TeleCore.Logging;
using TeleCore.Mod.Loader.Update;
using TeleCore.Primitive;
using TeleCore.Systems.RoomTracking;
using TeleCore.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere;

public class AtmosphericMapInfo : MapInformation
{
    private readonly Dictionary<Room, RoomComponent_Atmosphere> _compLookUp;
    //Scribing
    private AtmosphericScriber _scriber;

    public AtmosphericMapInfo(Map map) : base(map)
    {
        _scriber = new AtmosphericScriber(this);
        Cache = new AtmosphericCache(map);
        System = new AtmosphericSystem(map);
        Converters = new AtmosphericMapConverters();

        //
        _compLookUp = new Dictionary<Room, RoomComponent_Atmosphere>();
        AllAtmosphericRooms = new List<RoomComponent_Atmosphere>();

        //
        Renderer = new AtmosphereRenderer(map);
    }

    //
    public List<RoomComponent_Atmosphere> AllAtmosphericRooms { get; }

    public AtmosphereRenderer Renderer { get; }

    public AtmosphericSystem System { get; }

    public AtmosphericMapConverters Converters { get; }

    public AtmosphericVolume MapVolume => System.MapVolume;
    internal AtmosphericCache Cache { get; }

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
        System.Init(map);
        //map.GameConditionManager.RegisterCondition(GameConditionMaker.MakeConditionPermanent(AtmosDefOf.AtmosphericCondition));
    }

    public void RegenerateMapInfo()
    {
        TLog.Message("Regenerating map info...");
        System.Notify_Regenerate(Map.cellIndices
            .NumGridCells); //AllComps.Where(c => c.IsOutdoors).Sum(c => c.Room.CellCount)
    }

    //
    public override void Tick()
    {
        var tick = Find.TickManager.TicksGame;

        System.Tick(tick);
        Converters.Tick();
        Renderer.Tick();
    }

    #region Data

    public void Notify_UpdateRoomComp(RoomComponent_Atmosphere comp)
    {
        System.Notify_UpdateRoomComp(comp);
    }

    public void Notify_AddRoomComp(RoomComponent_Atmosphere comp)
    {
        if (_compLookUp.TryAdd(comp.Room, comp))
        {
            AllAtmosphericRooms.Add(comp);
            System.Notify_AddRoomComp(comp);
        }
        else
        {
            TLog.Warning($"Tried to add existin roomComp: {comp.Room.ID} | IsDisbanded? {comp.Disbanded}");
        }
    }

    public void Notify_RemoveRoomComp(RoomComponent_Atmosphere comp)
    {
        AllAtmosphericRooms.Remove(comp);
        _compLookUp.Remove(comp.Room);
        System.Notify_RemoveRoomComp(comp);
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
                    System.Notify_InterfaceBetweenRoomsChanged(tracker, neighbor, door, args.CompSignal);
                break;
        }
    }

    //Atmospher Scribing
    public void Notify_LoadedOutsideAtmosphere(DefValueStack<AtmosphericValueDef, double> stack)
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
        Renderer.AtmosphereDrawerUpdate();
        Renderer.Draw();
    }

    #endregion
}