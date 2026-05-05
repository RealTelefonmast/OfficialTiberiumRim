// Preserved from TeleCore/SpreadingGas/DynamicDataCacheMapInfo.cs (No TAC counterpart — GPU compute grid cache)

using TeleCore.Types.Exposables;
using TeleCore.Types.Structs;
using TeleCore.Types.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.MapComponents;

public class DynamicDataCacheMapInfo_TAE : MapInformation
{
    public DynamicDataCacheMapInfo_TAE(Map map) : base(map)
    {
        AtmosphericPassGrid = new ComputeGrid<float>(map, _ => 1f);
        LightPassGrid = new ComputeGrid<float>(map, _ => 1f);
        EdificeGrid = new ComputeGrid<uint>(map);
    }

    //Grid data
    public ComputeGrid<float> AtmosphericPassGrid { get; }
    public ComputeGrid<float> LightPassGrid { get; }
    public ComputeGrid<uint> EdificeGrid { get; }

    public ComputeBuffer AtmosphericBuffer => AtmosphericPassGrid.DataBuffer;
    public ComputeBuffer LightPassBuffer => LightPassGrid.DataBuffer;
    public ComputeBuffer EdificeBuffer => EdificeGrid.DataBuffer;

    //TODO: Let data get filled directly in map load - update buffer later
    public override void ThreadSafeInit()
    {
        AtmosphericPassGrid.ThreadSafeInit();
        LightPassGrid.ThreadSafeInit();
        EdificeGrid.ThreadSafeInit();

        AtmosphericPassGrid.UpdateBuffer();
        LightPassGrid.UpdateBuffer();
        EdificeGrid.UpdateBuffer();
    }

    internal void Notify_UpdateThingState(Thing thing)
    {
        var isBuilding = thing is Building;
        foreach (var pos in thing.OccupiedRect())
        {
            if (!isBuilding) continue;

            AtmosphericPassGrid.SetValue_Array(pos,
                TAE.AtmosphereUtility.DefaultAtmosphericPassPercentAtCell(pos, map));
            if (thing.def.IsEdifice())
                EdificeGrid.SetValue_Array(pos, 1);
            if (thing.def.blockLight)
                LightPassGrid.SetValue_Array(pos, 0);
        }
    }

    internal void Notify_ThingSpawned(Thing thing)
    {
        Notify_UpdateThingState(thing);
    }

    internal void Notify_ThingDespawned(Thing thing)
    {
        foreach (var pos in thing.OccupiedRect())
            if (thing is Building b)
            {
                if (AtmosphericPassGrid.IsReady)
                    AtmosphericPassGrid.ResetValue(pos, 1f);
                if (b.def.IsEdifice() && EdificeGrid.IsReady)
                    EdificeGrid.ResetValue(pos);
                if (b.def.blockLight && LightPassGrid.IsReady)
                    LightPassGrid.ResetValue(pos, 1f);
            }
    }
}

public class DynamicDataTracker_TAE : ThingTrackerComp
{
    private DynamicDataCacheMapInfo_TAE cacheMapInfo;

    //TODO: Update to use protected parent later
    public DynamicDataTracker_TAE(ThingTrackerMapInfo parent) : base(parent)
    {
    }

    public override void Notify_ThingRegistered(ThingStateChangedEventArgs args)
    {
        args.Thing.Map.GetMapInfo<TAE.DynamicDataCacheMapInfo>().Notify_ThingSpawned(args.Thing);
        args.Thing.Map.GetMapInfo<TAE.SpreadingGasGrid>().Notify_ThingSpawned(args.Thing);
    }

    public override void Notify_ThingDeregistered(ThingStateChangedEventArgs args)
    {
        args.Thing.Map.GetMapInfo<TAE.DynamicDataCacheMapInfo>().Notify_ThingDespawned(args.Thing);
    }

    public override void Notify_ThingSentSignal(ThingStateChangedEventArgs args)
    {
        switch (args.CompSignal)
        {
            case KnownCompSignals.FlickedOn:
            case KnownCompSignals.FlickedOff:
            case KnownCompSignals.PowerTurnedOn:
            case KnownCompSignals.PowerTurnedOff:
            case KnownCompSignals.RanOutOfFuel:
            case KnownCompSignals.Refueled:
            case "DoorOpened":
            case "DoorClosed":
            {
                args.Thing.Map.GetMapInfo<TAE.DynamicDataCacheMapInfo>().Notify_UpdateThingState(args.Thing);
            }
                break;
        }
    }
}