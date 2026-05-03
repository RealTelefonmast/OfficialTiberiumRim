using System.Diagnostics;

namespace TeleCore.Unsorted;

public static class TProfiler
{
    private static readonly bool _Disabled = true;

    static TProfiler()
    {
        Watch = new Stopwatch();
    }

    private static Stopwatch Watch { get; }
    private static string CurScope { get; set; }

    public static bool IsActive => !_Disabled && Watch.IsRunning;

    public static void Begin(string label = null)
    {
        //
        if (_Disabled) return;
        CurScope = label ?? null;
        Watch.Reset();
        Watch.Start();
    }

    public static void Check(string tag = null)
    {
        if (!Watch.IsRunning)
        {
            TLog.Warning("Cant check profiling while not running.");
            return;
        }

        TLogger.Log($"[{tag}]: {Watch.Elapsed.TotalMilliseconds}ms");
    }

    public static void End(string message = null)
    {
        if (!Watch.IsRunning) return;
        Watch.Stop();
        TLogger.Log(
            $"{(CurScope != null ? $"[{CurScope}]: " : null)}Profiling Result: {Watch.Elapsed.TotalMilliseconds}ms");
    }
}