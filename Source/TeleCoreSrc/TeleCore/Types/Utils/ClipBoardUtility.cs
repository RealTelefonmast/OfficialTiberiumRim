using System.Collections.Generic;

namespace TeleCore.Types.Utils;

/// <summary>
///     Dynamic Clipboard utility, allows you to save any type via a string tag, and retrieve it the same way.
/// </summary>
public static class ClipBoardUtility
{
    private static readonly Dictionary<string, object> Clipboard;

    static ClipBoardUtility()
    {
        Clipboard = new Dictionary<string, object>();
    }

    internal static void Notify_ClearData()
    {
        Clipboard.Clear();
    }

    public static bool IsActive(string clipBoardKey)
    {
        return Clipboard.ContainsKey(clipBoardKey);
    }

    public static T TryGetClipBoard<T>(string tag)
    {
        if (Clipboard.TryGetValue(tag, out var value)) return (T)value;
        return (T)(object)null;
    }

    public static void TrySetClipBoard<T>(string tag, T value)
    {
        if (Clipboard.TryAdd(tag, value))
        {
            //TLog.Debug($"Copied to clip-board for {tag}");
        }
    }
}