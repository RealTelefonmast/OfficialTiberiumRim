using System;
using TeleCore.Events.Args;
using TeleCore.FlowCore;
using TeleCore.Logging;

namespace TeleCore.Events;

//TODO: More events, game, map, incidents
// NOTE: The Rooms, ProjectileLaunched, and ClearData portions live here in TeleCore
// because they depend on TeleCore types (RoomTracker). The Things, Terrain, and Pawns
// portions live in TeleCore.Events assembly's GlobalEventHandler (partial class).
// Since partial classes cannot span assemblies, this is a separate static class.
public static class GlobalRoomEventHandler
{
    #region Things

    internal static void OnThingSpawned(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingSpawned?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to register spawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    internal static void OnThingDespawning(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingDespawning?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to deregister despawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    internal static void OnThingDespawned(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingDespawned?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to deregister despawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    internal static void OnThingSentSignal(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingSentSignal?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to send signal on thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    public static class Pawns
    {
        public static event PawnHediffChangedEvent? HediffChanged;

        //TODO:
        //public static event PawnEnteredRoomEvent PawnEnteredRoom;
        //public static event PawnLeftRoomEvent PawnLeftRoom;

        internal static void OnPawnHediffChanged(PawnHediffChangedEventArgs args)
        {
            try
            {
                HediffChanged?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to register hediff change on pawn: {args.Pawn}\n{ex.Message}");
            }
        }

        internal static void Clear()
        {
            HediffChanged = null;
        }
    }

    public static class Rooms
    {
        public static event RoomCreatedEvent Created;
        public static event RoomDisbandedEvent RoomDisbanded;
        public static event RegionStateEvent CachingRegionStateInfoRoomUpdate;
        public static event RegionStateEvent ResettingRegionStateInfoRoomUpdate;
        public static event RegionStateEvent GettingRegionStateInfoRoomUpdate;

        internal static void OnRoomCreated(RoomChangedArgs args)
        {
            try
            {
                Created?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error(
                    $"Error trying to register room creation: {args.RoomTracker.Room.ID}\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        internal static void OnRoomDisbanded(RoomChangedArgs args)
        {
            try
            {
                RoomDisbanded?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to notify disbanded room:\n{ex}");
            }
        }

        internal static void OnRoomReused(RoomChangedArgs args)
        {
            try
            {
                RoomDisbanded?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to notify reused room:\n{ex}");
            }
        }

        internal static void OnRegionStateCachedRoomUpdate(RegionStateChangedArgs action)
        {
            try
            {
                CachingRegionStateInfoRoomUpdate?.Invoke(action);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to notify reset region state:\n{ex}");
            }
        }

        internal static void OnRegionStateResetRoomUpdate(RegionStateChangedArgs action)
        {
            try
            {
                ResettingRegionStateInfoRoomUpdate?.Invoke(action);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to notify reset region state:\n{ex}");
            }
        }

        internal static void OnRegionStateGetRoomUpdate(RegionStateChangedArgs action)
        {
            try
            {
                GettingRegionStateInfoRoomUpdate?.Invoke(action);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to notify cached region state:\n{ex}");
            }
        }

        internal static void Clear()
        {
            Created = null;
            RoomDisbanded = null;
            CachingRegionStateInfoRoomUpdate = null;
            ResettingRegionStateInfoRoomUpdate = null;
            GettingRegionStateInfoRoomUpdate = null;
        }
    }

    #region Network

    public static class NetworkEvents<T> where T : FlowValueDef
    {
        public static event NetworkVolumeStateChangedEvent<T> NetworkVolumeStateChanged;

        public static void OnVolumeStateChange(FlowVolumeBase<T> flowVolume,
            VolumeChangedEventArgs<T>.ChangedAction action)
        {
            try
            {
                NetworkVolumeStateChanged?.Invoke(new VolumeChangedEventArgs<T>(action, flowVolume));
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to register volume change: {flowVolume}\n{ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    #endregion

    internal static void ClearData()
    {
        GlobalEventHandler.Things.Clear();
        GlobalEventHandler.Pawns.Clear();
        Rooms.Clear();
        GlobalEventHandler.Terrain.Clear();
    }

    #region Verbs

    public static event ProjectileLaunchedEvent ProjectileLaunched;

    internal static void OnProjectileLaunched(ProjectileLaunchedArgs args)
    {
        try
        {
            ProjectileLaunched?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify projectile launch: {args}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion
}

/*OLD REF
using System;
using TeleCore.FlowCore;

namespace TeleCore.Events;

//TODO: More events, game, map, incidents
// NOTE: The Rooms, ProjectileLaunched, and ClearData portions live here in TeleCore
// because they depend on TeleCore types (RoomTracker). The Things, Terrain, and Pawns
// portions live in TeleCore.Events assembly's GlobalEventHandler (partial class).
// Since partial classes cannot span assemblies, this is a separate static class.
public static class GlobalRoomEventHandler
{
    #region Things

    public static event ThingSpawnedEvent ThingSpawned;
    public static event ThingDespawnedEvent ThingDespawning;
    public static event ThingDespawnedEvent ThingDespawned;
    public static event ThingStateChangedEvent ThingSentSignal;

    #endregion


    #region Pawns

    public static event PawnHediffChangedEvent PawnHediffChanged;
    //TODO:
    //public static event PawnEnteredRoomEvent PawnEnteredRoom;
    //public static event PawnLeftRoomEvent PawnLeftRoom;

    #endregion


    public static event TerrainChangedEvent TerrainChanged;
    public static event CellChangedEvent CellChanged;


    #region Terrain

    public static void OnTerrainChanged(TerrainChangedEventArgs args)
    {
        try
        {
            TerrainChanged?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to register terrain change: {args.PreviousTerrain} -> {args.NewTerrain}\n{ex.Message}");
        }
    }

    #endregion

    //
    internal static void OnPawnHediffChanged(PawnHediffChangedEventArgs args)
    {
        try
        {
            PawnHediffChanged?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to register hediff change on pawn: {args.Pawn}\n{ex.Message}");
        }
    }

    internal static void ClearData()
    {
        // Things
        ThingSpawned = null;
        ThingDespawning = null;
        ThingDespawned = null;
        ThingSentSignal = null;

        // Pawns
        PawnHediffChanged = null;

        // Network
        //TODO:

        // Cell and Terrain
        TerrainChanged = null;
        CellChanged = null;
    }


    #region Things

    internal static void OnThingSpawned(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingSpawned?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to register spawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    internal static void OnThingDespawning(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingDespawning?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to deregister despawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    internal static void OnThingDespawned(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingDespawned?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to deregister despawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    internal static void OnThingSentSignal(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingSentSignal?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to send signal on thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region Verbs

    public static event ProjectileLaunchedEvent ProjectileLaunched;

    internal static void OnProjectileLaunched(ProjectileLaunchedArgs args)
    {
        try
        {
            ProjectileLaunched?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify projectile launch: {args}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion
}
*/

/* ANOTHER OLD REF
using System;
using TeleCore.Events;
using TeleCore.Utils;
using Verse;

namespace TeleCore.Absorbed.Events;

public static partial class GlobalEventHandler
{
    public static class Things
    {
        public static event ThingDiscardedEvent? Discarded;
        public static event ThingSpawnedEvent? Spawned;
        public static event ThingDespawnedEvent? Despawning;
        public static event ThingDespawnedEvent? Despawned;
        public static event ThingStateChangedEvent? SentSignal;

        public static void RegisterSpawned(ThingSpawnedEvent handler, Predicate<Thing> predicate)
        {
            Spawned += args =>
            {
                if (predicate(args.Thing))
                {
                    handler(args);
                }
            };
        }

        public static void RegisterDespawned(ThingDespawnedEvent handle, Predicate<Thing> predicate)
        {
            Despawned += args =>
            {
                if (predicate(args.Thing))
                {
                    handle(args);
                }
            };
        }

        internal static void OnSpawned(ThingStateChangedEventArgs args)
        {
            try
            {
                Spawned?.Invoke(args);
                Terrain.OnCellChanged(new CellChangedEventArgs(args));
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to register spawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        internal static void OnDespawning(ThingStateChangedEventArgs args)
        {
            try
            {
                Despawning?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to deregister despawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        internal static void OnDespawned(ThingStateChangedEventArgs args)
        {
            try
            {
                Despawned?.Invoke(args);
                Terrain.OnCellChanged(new CellChangedEventArgs(args));
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to deregister despawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        internal static void OnDiscarded(ThingStateChangedEventArgs args)
        {
            try
            {
                Discarded?.Invoke(args);
                Terrain.OnCellChanged(new CellChangedEventArgs(args));
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to discard thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        internal static void OnSentSignal(ThingStateChangedEventArgs args)
        {
            try
            {
                SentSignal?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to send signal on thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        internal static void Clear()
        {
            Spawned = null;
            Despawning = null;
            Despawned = null;
            SentSignal = null;
        }
    }

    public static class Terrain
    {
        public static event TerrainChangedEvent TerrainChanged;
        public static event CellChangedEvent CellChanged;

        internal static void OnCellChanged(CellChangedEventArgs args)
        {
            try
            {
                CellChanged?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to register cell change: {args.Cell}\n{ex.Message}");
            }
        }

        internal static void OnTerrainChanged(TerrainChangedEventArgs args)
        {
            try
            {
                TerrainChanged?.Invoke(args);
                CellChanged?.Invoke(new CellChangedEventArgs(args));
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to register terrain change: {args.PreviousTerrain} -> {args.NewTerrain}\n{ex.Message}");
            }
        }

        public static void Clear()
        {
            TerrainChanged = null;
            CellChanged = null;
        }
    }
}
*/