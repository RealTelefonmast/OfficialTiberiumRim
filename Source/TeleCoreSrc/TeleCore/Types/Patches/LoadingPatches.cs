using HarmonyLib;
using Verse;

namespace TeleCore.Types.Patches;

internal static class LoadingPatches
{
    [HarmonyPatch(typeof(TemperatureSaveLoad))]
    [HarmonyPatch(nameof(TemperatureSaveLoad.ApplyLoadedDataToRegions))]
    public static class ApplyLoadedDataToRegionsPatch
    {
        public static void Postfix(Map ___map)
        {
            ___map.GetMapInfo<AtmosphericMapInfo>().Notify_ApplyLoadedData();
        }
    }
}