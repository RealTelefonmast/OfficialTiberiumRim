using System;
using Verse;

namespace TeleCore.Loader;

[StaticConstructorOnStartup]
public class StaticEventHandler
{
    public static event Action? ApplicationQuitEvent;
    public static event Action? ClearingMapAndWorld;
    public static event Action? ClearData;

    static StaticEventHandler()
    {
        //TODO: ClearingMapAndWorld += GlobalUpdateEventHandler.ClearData;
    }

    internal static void CheckEventHandler<T>(ref T eventColl, bool clear = false) where T : Delegate
    {
        if (eventColl.GetInvocationList().Length > 0)
        {
            TLog.Warning($"Event {eventColl.Method.Name} has not been unsubscribed from properly.");
            if (clear)
            {
                eventColl = null!;
            }
        }
    }

    public static void OnClearingMapAndWorld()
    {
        try
        {
            ClearingMapAndWorld?.Invoke();
        }
        catch (Exception ex)
        {
            TLog.Error($"Error while trying to clear map and world data.\n{ex.Message}");
        }

    }

    internal static void OnApplicationQuit()
    {
        try
        {
            ApplicationQuitEvent?.Invoke();
        }
        catch (Exception ex)
        {
            TLog.Error($"Error while quitting app.\n{ex.Message}");
        }
    }
}