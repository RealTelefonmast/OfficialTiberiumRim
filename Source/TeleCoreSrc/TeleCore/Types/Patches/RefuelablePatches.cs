using HarmonyLib;
using RimWorld;
using TeleCore.MapComponents;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Types.Patches;

internal static class RefuelablePatches
{
    [HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.HasFuel), MethodType.Getter)]
    private static class CompRefuelable_HasFuel_Patch
    {
        private static void Postfix(CompRefuelable __instance, ref bool __result)
        {
            if (!__result) return;
            if (__instance.parent is not ThingWithComps thing) return;
            if (OxygenUtility.IsBurner(thing))
            {
                var conv = thing.Map.GetMapInfo<AirMapInfo>().ConverterFor(thing);
                __result &= conv.IsActive; //.HasOxygen(thing);
            }
        }
    }
}