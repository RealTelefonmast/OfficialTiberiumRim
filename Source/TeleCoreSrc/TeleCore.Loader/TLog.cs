using UnityEngine;
using Verse;

namespace TeleCore.Loader;

public static class TLog
{
    private static readonly Color LogBlue = new(0.17f, 0.74f, 1f);
    private static readonly Color LogGreen = new(0.16f, 0.71f, 0.45f);
    
    public static void Error(string msg, string tag = null)
    {
        Log.Error($"{"[TELE][ERROR]".Colorize(LogBlue)} {msg}");
    }

    public static void ErrorOnce(string msg, int id)
    {
        Log.ErrorOnce($"{"[TELE]".Colorize(LogBlue)} {msg}", id);
    }

    public static void Warning(string msg)
    {
        Log.Warning($"{"[TELE]".Colorize(LogBlue)} {msg}");
    }

    public static void Message(string msg, Color color)
    {
        Log.Message($"{"[TELE]".Colorize(color)} {msg}");
    }

    public static void Message(string msg)
    {
        Log.Message($"{"[TELE]".Colorize(LogBlue)} {msg}");
    }

    public static void Debug(string msg, bool flag = true)
    {
        if (flag) Log.Message($"{"[TELE-Debug]".Colorize(LogGreen)} {msg}");
    }

    public static void DebugSuccess(string msg)
    {
        Log.Message($"{"[TELE-Debug]".Colorize(Color.green)} {msg}");
    }
}