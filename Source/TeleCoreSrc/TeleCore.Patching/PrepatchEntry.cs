using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Prepatcher;
using Mono.Cecil;
using Verse;

namespace TeleCore.Patching;

/// <summary>
/// I blame Bradson (use his mods they are awesome)
/// Inspired by his Prepatch Wrapper
/// https://github.com/bbradson/Performance-Fish/blob/main/Source/PerformanceFish/Prepatching/FreePatchEntry.cs
/// </summary>
public static class PrepatchEntry
{
    [FreePatch]
    [UsedImplicitly]
    public static void Start(ModuleDefinition module)
    {
        //TODO: Doorstep currently doesnt load referenced assemblies without Prepatcher attached
        try
        {
            StartInt(module);
        }
        catch (Exception ex)
        {
            Log.Message($"TeleCore:Prepatching Failed: {ex}");
        }
    }

    private static void CheckLoadMissingAssemblies()
    {
        List<string> desired = new List<string>();
        foreach (var mcp in LoadedModManager.runningMods)
        {
            var telePatchFiles = ModContentPack.GetAllFilesForModPreserveOrder(mcp, "Assemblies/", e => e.ToLower() == ".tpf");
            foreach (var file in telePatchFiles)
            {
                Log.Message("Reading From: " + file); //Read text from file
                var lines = File.ReadAllLines(file.Item2.FullName);
                desired.AddRange(lines);
            }
        }
    }

    private static void StartInt(ModuleDefinition module)
    {
        var types = typeof(TelePatch).AllSubclassesNonAbstract();
        if (types.Count > 0)
        {
            foreach (var type in types)
            {
                Log.Message("Patch: " + type.FullName);
            }
        }
    }
}