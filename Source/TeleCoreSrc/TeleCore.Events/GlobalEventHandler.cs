using System;
using TeleCore.Events.Args;
using TeleCore.Loader;
using TeleCore.Shared;
using Verse;

namespace TeleCore.Events;

[StaticConstructorOnStartup]
public static partial class GlobalEventHandler
{
    static GlobalEventHandler()
    {
        StaticEventHandler.ClearData += Things.Clear;
        StaticEventHandler.ClearData += Pawns.Clear;
    }
    
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