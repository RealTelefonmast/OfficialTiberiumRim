using TeleCore.Static;
using UnityEngine;

namespace TeleCore.Logging;

public static class TLog
{
    public static readonly ModLogger Logger = LoggerRegistry.Register(
        "TeleCore", new ModLogger("[TELE]", TColor.NiceBlue, TColor.Green));

    public static void Error(string msg, string tag = null) => Logger.Error(msg);
    public static void ErrorOnce(string msg, int id)        => Logger.ErrorOnce(msg, id);
    public static void Warning(string msg)                  => Logger.Warning(msg);
    public static void Message(string msg)                  => Logger.Message(msg);
    public static void Message(string msg, Color color)     => Logger.Message(msg, color);
    public static void Debug(string msg, bool flag = true)  => Logger.Debug(msg, flag);
    public static void DebugSuccess(string msg)             => Logger.DebugSuccess(msg);
    public static void DebugOnce(string msg, int hash)      => Logger.DebugOnce(msg, hash);
}