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

    public static ModLogger Get(string id) => _loggers[id];

    public static bool TryGet(string id, out ModLogger logger) =>
        _loggers.TryGetValue(id, out logger);
}
