using HarmonyLib;
using RimWorld.Planet;
using TeleCore;
using TR.GameParts.WorldInfos;
using TR.Util;

namespace TR.Patches;

public class MapWorldPatches
{
    [HarmonyPatch(typeof(MapParent), nameof(MapParent.ShouldRemoveMapNow))]
    [HarmonyPatch(typeof(Settlement), nameof(MapParent.ShouldRemoveMapNow))]
    public static class MapParentShouldRemoveMapNow_Patch
    {
        public static void Postfix(MapParent __instance, ref bool alsoRemoveWorldObject, ref bool __result)
        {
            var isWatched = TRUtils.Tiberium().GetWorldInfo<WorldDataInfo>().IsSpiedOn(__instance.Map);
            if (isWatched)
            {
                __result = false;
                alsoRemoveWorldObject = false;
            }
        }
    }
}