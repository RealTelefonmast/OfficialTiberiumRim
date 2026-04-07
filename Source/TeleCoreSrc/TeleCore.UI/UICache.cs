using System.Collections.Generic;
using TeleCore.Loader;

namespace TeleCore.TeleUI;

internal static class UICache
{
    private const string DefaultContext = "TeleCoreUIMainContext";
    private static readonly Dictionary<string, Dictionary<string, string>> _contextStringBuffers = new ();

    public static string GetBuffer(string ctrlName, string? context = null)
    {
        return GetBuffer(ctrlName, "", context);
    }

    public static string GetBufferFormat(string ctrlName, string format, string? context = null)
    {
        var value = GetBuffer(ctrlName, "", context);
        return string.Format(format, value);
    }
    
    public static string GetBuffer(string ctrlName, object initialValue, string? context = null)
    {
        Dictionary<string, string> _stringBuffers;
        context ??= DefaultContext;

        if (_contextStringBuffers.TryGetValue(context, out _stringBuffers))
        {
            if (_stringBuffers.TryGetValue(ctrlName, out var buffer))
            {
                return buffer;
            }
        }

        _stringBuffers ??= new Dictionary<string, string>();
        _contextStringBuffers[context] = _stringBuffers;
        return _stringBuffers[ctrlName] = initialValue.ToString();
    }

    public static void ClearContext(string context = DefaultContext)
    {
        if (_contextStringBuffers.TryGetValue(context, out var stringBuffers))
        {
            stringBuffers.Clear();
        }
    }

    public static string SetBuffer(string ctrlName, string newValue, string? context = null)
    {
        Dictionary<string, string> _stringBuffers;
        context ??= DefaultContext;
        if (_contextStringBuffers.TryGetValue(context, out _stringBuffers))
        {
            if (_stringBuffers.TryGetValue(ctrlName, out var buffer))
            {
                _stringBuffers[ctrlName] = newValue;
                return newValue;
            }
            TLog.Warning("Tried to set buffer in context that doesn't exist: " + context);
            return newValue;
        }
        TLog.Warning("Tried to set buffer for control that doesn't exist: " + ctrlName);
        return newValue;
    }
}