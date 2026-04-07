using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TeleCore.Logging;
using Verse;

namespace TR;

[StaticConstructorOnStartup]
public static class TeleCoreStaticStartup
{
    static TeleCoreStaticStartup()
    {
        StopWatch = new Stopwatch();
        TLog.Message("Startup Init...");

        //Init Startup types
        foreach (var type in GenTypes.AllTypesWithAttribute<TeleCoreStartupClassAttribute>())
            try
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
            catch (Exception e)
            {
                TLog.Error($"Failed to run startup constructor for {type}: {e.Message}");
            }

        TLog.Message($"TeleCore: Loaded Modules {StopWatch.ElapsedMilliseconds}ms");

        //DefID Validator
        TeleStartUtils.DefIDValidation();
        TeleStartUtils.ExecuteDefInjectors();

        OnStartup?.Invoke();

        //MP Hook
        TLog.Message("Multiplayer: Outdated.");
        //TLog.Message($"Multiplayer: {(MP.enabled ? "Enabled - Adding MP hooks..." : "Disabled")}");
        // if (MP.enabled)
        // {
        //     try
        //     {
        //         MP.RegisterAll();
        //     }
        //     catch (Exception ex)
        //     {
        //         TLog.Error($"Failed to register MP hooks: {ex.Message}");
        //     }
        // }
    }

    internal static Stopwatch StopWatch { get; }
    public static event Action? OnStartup;
}