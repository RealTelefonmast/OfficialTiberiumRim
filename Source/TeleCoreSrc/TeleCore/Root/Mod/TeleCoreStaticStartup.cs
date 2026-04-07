using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeleCore.Logging;
using TeleCore.Utils;
using Verse;
//using Multiplayer.API;

namespace TeleCore.Mod;

[StaticConstructorOnStartup]
internal static class TeleCoreStaticStartup
{
    static TeleCoreStaticStartup()
    {
        TLog.Message("Startup Init...");
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

    public static List<(string TypeName, Assembly Assembly)> FindDuplicateTypes()
    {
        var types = GenTypes.AllTypes; ;
        var duplicates = new List<(string TypeName, Assembly Assembly)>();

        foreach (var type in types)
        {
            // Get type details
            var typeName = type.FullName;
            var assembly = type.Assembly;

            duplicates.Add((typeName, assembly));
        }

        var duplicateTypes = duplicates.GroupBy(x => x)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        // Return only the types that occur more than once
        return duplicateTypes;
    }

    /*
    internal static void LoadStaticTranslationLibraries()
    {
        foreach (var type in GenTypes.AllTypesWithAttribute<StaticTranslationLibraryAttribute>())
        {
            try
            {
                TLog.Debug("Loading type for tranlsation..");
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
            catch (Exception ex)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Error in static translation library of ",
                    type,
                    ": ",
                    ex
                }));
            }
        }
    }
    */
}