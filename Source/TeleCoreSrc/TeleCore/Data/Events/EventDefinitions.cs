using TeleCore.FlowCore;

namespace TeleCore.Events;

//Things
public delegate void ThingSpawnedEvent(ThingStateChangedEventArgs args);
public delegate void ThingDespawnedEvent(ThingStateChangedEventArgs args);
public delegate void ThingStateChangedEvent(ThingStateChangedEventArgs args);
public delegate void TerrainChangedEvent(TerrainChangedEventArgs args);
public delegate void CellChangedEvent(CellChangedEventArgs args);

//Network
public delegate void NetworkVolumeStateChangedEvent<T>(VolumeChangedEventArgs<T> args) where T : FlowValueDef;

//Hediffs
public delegate void PawnHediffChangedEvent(PawnHediffChangedEventArgs args);

//FX Events
/*
public delegate CompPowerTrader FXGetPowerProviderEvent(FXLayerArgs args);
public delegate bool FXGetShouldDrawEvent(FXLayerArgs args);
public delegate float FXGetOpacityEvent(FXLayerArgs args);
public delegate float? FXGetRotationEvent(FXLayerArgs args);
public delegate float? FXGetAnimationSpeedEvent(FXLayerArgs args);
public delegate Color? FXGetColorEvent(FXLayerArgs args);
public delegate Vector3? FXGetDrawPositionEvent(FXLayerArgs args);
public delegate Action<FXLayer> FXGeActionEvent(FXLayerArgs args);
public delegate bool FXShouldThrowEffectsEvent(EffecterLayerArgs args);
*/
public delegate void OnEffectSpawnedEvent(FXEffecterSpawnedEventArgs spawnedEventArgs);

public delegate void NetworkChangedEvent(NetworkChangedEventArgs args);

public delegate void MovedEventHandler(object sender, MovedEventArgs args);

//Tele Specific
public delegate void EntityTickedEvent();

//Rooms/Regions
public delegate void RoomCreatedEvent(RoomChangedArgs args);
public delegate void RoomDisbandedEvent(RoomChangedArgs args);
public delegate void RegionStateEvent(RegionStateChangedArgs args);

//Verbs
public delegate void ProjectileLaunchedEvent(ProjectileLaunchedArgs args);
