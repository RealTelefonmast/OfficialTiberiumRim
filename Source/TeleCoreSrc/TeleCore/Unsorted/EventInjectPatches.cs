using HarmonyLib;
using Verse;

namespace TeleCore.Unsorted;

public static class EventInjectPatches
{
    [HarmonyPatch(typeof(Hediff))]
    [HarmonyPatch(nameof(Hediff.PostAdd))]
    public static class ProjectileArcHeightFactorPatch
    {
        public static void Postfix(Hediff __instance, DamageInfo? dinfo)
        {
            GlobalEventHandler.Pawns.OnPawnHediffChanged(new PawnHediffChangedEventArgs(__instance, dinfo));
        }
    }
}