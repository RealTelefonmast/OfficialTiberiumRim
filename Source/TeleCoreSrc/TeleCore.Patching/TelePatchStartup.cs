using TeleCore.Loader;
using Verse;

namespace TeleCore.Patching;

[TeleCoreStartupClass]
public static class TelePatchStartup
{
    static TelePatchStartup()
    {
        TLog.Message($"TeleCore.Patching...");
        
        foreach (var pack in LoadedModManager.RunningModsListForReading)
        {
            foreach (var assembly in pack.assemblies.loadedAssemblies)
            {
                
                
            }
        }
        
    }
}