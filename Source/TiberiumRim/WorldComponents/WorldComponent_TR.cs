using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using TiberiumRim;
using TR.SuperWeapon;
using TR.WorldInfos;
using Verse;

namespace TR.World;

public class WorldComponent_TR : WorldComponent
{
    private readonly Dictionary<Type, WorldInformation> _worldInfoByType = new();
    private List<WorldInformation> _worldInfos = new();

    public ASATNetwork AttackSatelliteNetwork;

    //Discovery
    public DiscoveryTable DiscoveryTable;
    public List<SuperWeaponInfo> SuperWeapons = new();

    public WorldComponent_TR(World world) : base(world)
    {
        AttackSatelliteNetwork = new ASATNetwork();
        DiscoveryTable = new DiscoveryTable();

        foreach (var info in typeof(WorldInformation).AllSubclassesNonAbstract())
            try
            {
                var item = (WorldInformation)Activator.CreateInstance(info, world);
                _worldInfos.Add(item);
            }
            catch (Exception ex)
            {
                TRLog.Error($"Could not instantiate a WorldInfo of type {info}:\n{ex}");
            }

        RebuildInfoByType();
        DiscoveryTable ??= new DiscoveryTable();
    }

    // Typed convenience accessors
    public TiberiumWorldInfo TiberiumInfo => GetWorldInfo<TiberiumWorldInfo>();
    public GroundZeroInfo GroundZeroInfo => GetWorldInfo<GroundZeroInfo>();
    public SatelliteInfo SatelliteInfo => GetWorldInfo<SatelliteInfo>();
    public SuperWeaponInfo SuperWeaponInfo => GetWorldInfo<SuperWeaponInfo>();
    public TRGameSettingsInfo GameSettings => GetWorldInfo<TRGameSettingsInfo>();

    // Incident locks
    public bool AllowTRInit => GroundZeroInfo.HasGroundZero;
    public bool AllowNewMeteorites => TiberiumDefOf.MineralAnalysis.IsFinished;

    public T GetWorldInfo<T>() where T : WorldInformation
    {
        return (T)_worldInfoByType[typeof(T)];
    }

    private void RebuildInfoByType()
    {
        _worldInfoByType.Clear();
        foreach (var info in _worldInfos)
            _worldInfoByType.Add(info.GetType(), info);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref _worldInfos, "worldInfos", LookMode.Deep, world);
        Scribe_Deep.Look(ref DiscoveryTable, "DiscoveryTable");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            RebuildInfoByType();
            DiscoveryTable ??= new DiscoveryTable();
        }
    }

    public override void WorldComponentTick()
    {
        foreach (var info in _worldInfos) info.InfoTick();
    }

    public void Notify_TiberiumArrival(Map map)
    {
        TiberiumInfo.SpawnTiberiumTile(map.Tile);
    }

    public void Notify_BuildingSpawned(TRBuildingPrototype building)
    {
        foreach (var info in _worldInfos) info.Notify_BuildingSpawned(building);
    }

    public void Notify_RegisterWorldObject(GlobalTargetInfo worldObjectOrThing)
    {
        foreach (var info in _worldInfos) info.Notify_RegisterWorldObject(worldObjectOrThing);
    }

    public void Notify_EventHappened(string tag, IIncidentTarget location = null)
    {
        foreach (var info in _worldInfos) info.Notify_EventHappened(tag, location);
    }

    public void TryRegisterSuperweapon(TRBuilding building)
    {
        var superWep = building.def.superWeapon;
        if (superWep == null) return;
        var info = (SuperWeaponInfo)Activator.CreateInstance(superWep.worker);
        info.building = building;
        info.ticksUntilReady = superWep.chargeTime.SecondsToTicks();

        SuperWeapons.Add(info);
    }

    public void Notify_SuperWeaponFired(TRThingDef def)
    {
    }
}