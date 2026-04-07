using HarmonyLib;
using TeleCore.Events;
using Verse;

namespace TeleCore.Patches;

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