using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace TeleCore.Unsorted;

[TeleCoreStartupClass]
public static class PatchFileGenerator
{
    static PatchFileGenerator()
    {
        //Generate patch files on game start in the appdata game temp files
        foreach (var mcp in LoadedModManager.runningMods)
        {
            
        }
    }

    private static IEnumerable<Assembly> GetAssemblies(ModAssemblyHandler assemblies)
    {
        foreach (var assembly in assemblies.loadedAssemblies)
        {
            yield return assembly;
        }
    }
}