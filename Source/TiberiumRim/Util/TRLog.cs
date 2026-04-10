using TeleCore.Logging;
using UnityEngine;

namespace TR;

public static class TRLog
{
    public static readonly ModLogger Logger = LoggerRegistry.Register(
        "TiberiumRim", new ModLogger("[TR]", TRColor.Green));

    public static void Error(string msg)
    {
        Logger.Error(msg);
    }

    public static void ErrorOnce(string msg, int id)
    {
        Logger.ErrorOnce(msg, id);
    }

    public static void Warning(string msg)
    {
        Logger.Warning(msg);
    }

    public static void Message(string msg)
    {
        Logger.Message(msg);
    }

    public static void Message(string msg, Color color)
    {
        Logger.Message(msg, color);
    }

    public static void Debug(string msg, bool flag = true)
    {
        Logger.Debug(msg, flag);
    }
}