using System;

namespace TeleCore.Unsorted;

public static class GlobalUpdateEventHandler
{
    //GameTick is for game logic, GameUITick is for UI logic during a loaded game
    public static event Action GameTick;
    public static event Action GameUITick;
    public static event Action UITick;

    public static void ClearData()
    {
        StaticEventHandler.CheckEventHandler(ref GameTick, true);
        StaticEventHandler.CheckEventHandler(ref GameUITick, true);
    }

    public static void OnGameTick()
    {
        try
        {
            GameTick?.Invoke();
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to process game tick event\n{ex.Message}");
        }
    }

    public static void OnUITick()
    {
        try
        {
            UITick?.Invoke();
            GameUITick?.Invoke();
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to process UI tick event\n{ex.Message}");
        }
    }
}

