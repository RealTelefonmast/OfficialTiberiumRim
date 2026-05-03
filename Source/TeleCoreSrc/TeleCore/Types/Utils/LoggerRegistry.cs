using System.Collections.Generic;

namespace TeleCore.Unsorted;

public static class LoggerRegistry
{
    private static readonly Dictionary<string, ModLogger> _loggers = new();

    public static ModLogger Register(string id, ModLogger logger)
    {
        _loggers[id] = logger;
        return logger;
    }

    public static ModLogger Get(string id)
    {
        return _loggers[id];
    }

    public static bool TryGet(string id, out ModLogger logger)
    {
        return _loggers.TryGetValue(id, out logger);
    }
}