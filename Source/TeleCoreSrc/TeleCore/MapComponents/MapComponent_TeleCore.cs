using System;
using System.Collections.Generic;
using TeleCore.Unsorted;
using Verse;

//TODO: Resolve TiberiumRim namespace references after consolidation
//using TiberiumRim;
namespace TeleCore.MapComponents;

public class MapComponent_TeleCore : MapComponent
{
    private readonly Dictionary<Type, MapInformation> mapInfoByType = new();
    private List<MapInformation> allMapInfos = new();

    public MapComponent_TeleCore(Verse.Map map) : base(map)
    {
        StaticData.Notify_NewTeleMapComp(this);
        FillMapInformations();
    }

    public PipeNetworkMapInfo NetworkInfo { get; private set; }
    public ThingGroupCacheInfo ThingGroupCacheInfo { get; private set; }
    public ThingTrackerMapInfo ThingTrackerMapInfo { get; private set; }

    public T GetMapInfo<T>() where T : MapInformation
    {
        return (T)mapInfoByType[typeof(T)];
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref allMapInfos, "mapInfos", LookMode.Deep, map);

        FillMapInformations();
    }

    private void FillMapInformations()
    {
        allMapInfos.RemoveAll(m => m == null);
        foreach (var type in typeof(MapInformation).AllSubclassesNonAbstract())
        {
            if (allMapInfos.Any(m => m.GetType() == type)) continue;

            try
            {
                var item = (MapInformation)Activator.CreateInstance(type, map);
                allMapInfos.Add(item);
            }
            catch (Exception ex)
            {
                TLog.Error($"Could not instantiate a MapInformation of type {type}:\n{ex}");
            }
        }

        //
        mapInfoByType.Clear();
        foreach (var mapInfo in allMapInfos)
            mapInfoByType.Add(mapInfo.GetType(), mapInfo);

        //
        NetworkInfo = (PipeNetworkMapInfo) mapInfoByType[typeof(PipeNetworkMapInfo)];
        ThingGroupCacheInfo = (ThingGroupCacheInfo) mapInfoByType[typeof(ThingGroupCacheInfo)];
        ThingTrackerMapInfo = (ThingTrackerMapInfo) mapInfoByType[typeof(ThingTrackerMapInfo)];
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        for (var i = 0; i < allMapInfos.Count; i++)
        {
            var info = allMapInfos[i];
            info.InfoInit();
        }

        //
        LongEventHandler.QueueLongEvent(ThreadSafeFinalize, string.Empty, false, null, false);
    }

    private void ThreadSafeFinalize()
    {
        for (var i = 0; i < allMapInfos.Count; i++)
        {
            var info = allMapInfos[i];
            info.ThreadSafeInit();
        }
    }

    public override void MapGenerated()
    {
        for (var i = 0; i < allMapInfos.Count; i++)
        {
            var info = allMapInfos[i];
            info.OnMapGenerated();
        }
    }

    //Updates
    public override void MapComponentTick()
    {
        for (var i = 0; i < allMapInfos.Count; i++)
        {
            var info = allMapInfos[i];
            info.Tick();
        }
    }

    public override void MapComponentOnGUI()
    {
        for (var i = 0; i < allMapInfos.Count; i++)
        {
            var info = allMapInfos[i];
            info.UpdateOnGUI();
        }
    }

    public override void MapComponentUpdate()
    {
        for (var i = 0; i < allMapInfos.Count; i++)
        {
            var info = allMapInfos[i];
            info.Update();
        }
    }

    internal void TeleMapSingleTick()
    {
        for (var i = 0; i < allMapInfos.Count; i++)
            allMapInfos[i].TeleTick();
    }


    internal void TeleMapUpdate()
    {
        for (var i = 0; i < allMapInfos.Count; i++) allMapInfos[i].TeleUpdate();
    }
}