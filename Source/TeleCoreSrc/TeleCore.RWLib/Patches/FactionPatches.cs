using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace TeleCore.Mod.Loading;

internal static class FactionPatches
{
    [HarmonyPatch]
    internal static class GetInitialGoodwillPatch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.FirstMethod(typeof(Faction), method => method.Name.Contains("GetInitialGoodwill"));
        }

        public static bool Prefix(Faction a, Faction b, ref int __result)
        {
            var extension = a.def.GetModExtension<FactionDefExtension>();
            if (extension is not { enemyTo.Count: > 0 }) return true;

            for (var i = 0; i < extension.enemyTo.Count; i++)
            {
                var enemy = extension.enemyTo[i];
                if (enemy.Def == b.def)
                {
                    __result = enemy.Value;
                    break;
                }
            }
            return false;
        }
    }
}