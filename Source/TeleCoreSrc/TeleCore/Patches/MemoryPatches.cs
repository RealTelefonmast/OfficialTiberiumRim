using HarmonyLib;
using TeleCore.Events;
using Verse;
using Verse.Profile;

namespace TeleCore.Patches;

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