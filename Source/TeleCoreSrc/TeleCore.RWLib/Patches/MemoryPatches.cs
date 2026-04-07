using HarmonyLib;
using TeleCore.Loading;
using TeleCore.Events;
using TeleCore.Loader;
using Verse;
using Verse.Profile;

namespace TeleCore;

internal static class MemoryPatches
{
    [HarmonyPatch(typeof(MemoryUtility))]
    [HarmonyPatch(nameof(MemoryUtility.ClearAllMapsAndWorld))]
    internal static class MemoryUtility_ClearAllMapsAndWorldPatch
    {
        public static void Postfix()
        {
            StaticEventHandler.OnClearingMapAndWorld();

            //
            UnloadUtility.MemoryUnloadEvent?.Invoke();

            if (UnloadUtility.MemoryUnloadEventThreadSafe != null)
                LongEventHandler.ExecuteWhenFinished(UnloadUtility.MemoryUnloadEventThreadSafe);
        }
    }
}