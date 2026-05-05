// Preserved from TeleCore/Data/Map/AtmosphericMapInfo.cs (TAE mid-version ~183L)

using System.Collections.Generic;
using RimWorld;
using TeleCore.Defs;
using TeleCore.RoomComponents;
using TeleCore.Types;
using TeleCore.Types.Exposables;
using TeleCore.Types.Structs;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.MapComponents;

public class AtmosphericMapInfo_TAE_Data : MapInformation
{
    private readonly Dictionary<Room, RoomComponent_Atmosphere> _compLookUp;

    //System for room-nased atmospheric flow

    //Scribing
    private AtmosphericCache _cache;

    public AtmosphericMapInfo_TAE_Data(Map map) : base(map)
    {
        _cache = new AtmosphericCache(this);
        System = new AtmosphericSystem(map);

        //mapContainer = new AtmosphericContainer(null, AtmosResources.DefaultAtmosConfig(map.cellIndices.NumGridCells));

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

    public AtmosphericVolume MapVolume => System.MapVolume;

    public override void ExposeDataExtra()
    {
        Scribe_Deep.Look(ref _cache, "atmosCache", map);
    }

    //
    public RoomComponent_Atmosphere ComponentAt(IntVec3 pos)
    {
        var room = pos.GetRoomFast(Verse.Map);
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
        System.Notify_Regenerate(Verse.Map.cellIndices
            .NumGridCells); //AllComps.Where(c => c.IsOutdoors).Sum(c => c.Room.CellCount)
    }

    //
    public override void Tick()
    {
        var tick = Find.TickManager.TicksGame;

        System.Tick(tick);
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
                var roomA = positionA.GetRoomFast(Verse.Map);
                var roomB = positionB.GetRoomFast(Verse.Map);

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
        //.Cache.scriber.ApplyLoadedDataToRegions();
    }

    public void TrySpawnGasAt(IntVec3 cell, SpreadingGasTypeDef gasType, float value)
    {
        Verse.Map.GetMapInfo<SpreadingGasGrid>().Notify_SpawnGasAt(cell, gasType, value);
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