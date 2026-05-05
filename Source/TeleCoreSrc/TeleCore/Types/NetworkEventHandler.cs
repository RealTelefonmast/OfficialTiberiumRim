using System;
using TeleCore.Defs;
using TeleCore.Types.Structs;
using TeleCore.Types.Utils;

namespace TeleCore.Types;

public class NetworkEventHandler<T> where T : FlowValueDef
{
    public static event NetworkVolumeStateChangedEvent<T> NetworkVolumeStateChanged;

    internal static void OnVolumeStateChange(FlowVolumeBase<T> flowVolume,
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