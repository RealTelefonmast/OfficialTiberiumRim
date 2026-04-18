using HarmonyLib;

namespace TeleCore.Unsorted;

internal static class LoadingPatches
{
    [HarmonyPatch(typeof(TemperatureSaveLoad))]
    [HarmonyPatch(nameof(TemperatureSaveLoad.ApplyLoadedDataToRegions))]
    public static class ApplyLoadedDataToRegionsPatch
    {
        public static void Postfix(Verse.Map ___map)
        {
            ___map.GetMapInfo<AtmosphericMapInfo>().Notify_ApplyLoadedData();
            
        }
    }
}