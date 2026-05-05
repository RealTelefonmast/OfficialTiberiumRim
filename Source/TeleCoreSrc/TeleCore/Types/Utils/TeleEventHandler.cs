using System;

namespace TeleCore.Types.Utils;

public static class TeleEventHandler
{
    public static event Action<float> Tick;
    public static event EntityTickedEvent EntityTicked;

    internal static void OnEntityTicked()
    {
        try
        {
            EntityTicked?.Invoke();
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to tick entities:\n{ex.Message}");
        }
    }

    public static void OnTick(float tickRate)
    {
        try
        {
            Tick?.Invoke(tickRate);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to tick:\n{ex.Message}");
        }
    }
}