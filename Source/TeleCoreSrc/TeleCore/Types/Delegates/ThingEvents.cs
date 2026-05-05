using TeleCore.Types.Structs;

namespace TeleCore.Types.Delegates;

//Things
public delegate void ThingSpawnedEvent(ThingStateChangedEventArgs args);

public delegate void ThingDespawnedEvent(ThingStateChangedEventArgs args);

public delegate void ThingDiscardedEvent(ThingStateChangedEventArgs args);

public delegate void ThingStateChangedEvent(ThingStateChangedEventArgs args);

public delegate void TerrainChangedEvent(TerrainChangedEventArgs args);

public delegate void CellChangedEvent(CellChangedEventArgs args);

//Rooms/Regions
//TODO: RoomCreatedEvent and RoomDisbandedEvent require RoomChangedArgs which depends on RoomTracker (TeleCore assembly) - defined in TeleCore EventDefinitions
//public delegate void RoomCreatedEvent(RoomChangedArgs args);
//public delegate void RoomDisbandedEvent(RoomChangedArgs args);
public delegate void RegionStateEvent(RegionStateChangedArgs args);

//Verbs
public delegate void ProjectileLaunchedEvent(ProjectileLaunchedArgs args);

//Tele Specific
public delegate void EntityTickedEvent();

//TODO: The following delegates require args types from TeleCore main assembly - defined in TeleCore EventDefinitions
//public delegate void OnEffectSpawnedEvent(FXEffecterSpawnedEventArgs spawnedEventArgs);
//public delegate void NetworkChangedEvent(NetworkChangedEventArgs args);
//public delegate void MovedEventHandler(object sender, MovedEventArgs args);