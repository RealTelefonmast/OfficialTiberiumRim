using HarmonyLib;
using RimWorld;
using TeleCore.Events.Args;
using TeleCore.Shared;
using Verse;

namespace TeleCore.Events.Patches;

internal static class ThingEvent_Patches
{
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch(nameof(Thing.SpawnSetup))]
    public static class Thing_SpawnSetupPatch
    {
        public static void Postfix(Thing __instance)
        {
            GlobalEventHandler.Things.OnSpawned(new ThingStateChangedEventArgs(ThingChangeFlag.Spawned, __instance));
        }
    }

    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch(nameof(Thing.DeSpawn))]
    public static class Thing_DeSpawnPatch
    {
        public static bool Prefix(Thing __instance)
        {
            GlobalEventHandler.Things.OnDespawning(new ThingStateChangedEventArgs(ThingChangeFlag.Despawning,
                __instance));
            return true;
        }

        public static void Postfix(Thing __instance)
        {
            GlobalEventHandler.Things.OnDespawned(new ThingStateChangedEventArgs(ThingChangeFlag.Despawned, __instance));
        }
    }
    
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch(nameof(Thing.Discard))]
    public static class Thing_DiscardPatch
    {
        public static void Postfix(Thing __instance)
        {
            GlobalEventHandler.Things.OnDiscarded(new ThingStateChangedEventArgs(ThingChangeFlag.Discarded, __instance));
        }
    }
    
    [HarmonyPatch(typeof(ThingWithComps))]
    [HarmonyPatch(nameof(ThingWithComps.BroadcastCompSignal))]
    public static class ThingWithComps_BroadcastCompSignalPatch
    {
        public static void Postfix(ThingWithComps __instance, string signal)
        {
            //Event Handling
            GlobalEventHandler.Things.OnSentSignal(new ThingStateChangedEventArgs(ThingChangeFlag.SentSignal, __instance, signal));
        }
    }
    
    [HarmonyPatch(typeof(Building_Door))]
    [HarmonyPatch(nameof(Building_Door.CheckClearReachabilityCacheBecauseOpenedOrClosed))]
    public static class Building_Door_CheckClearReachabilityCacheBecauseOpenedOrClosed_Patch
    {
        public static void Postfix(Building_Door __instance)
        {
            //This method is called whenever a door is opened or closed, so we can use it to send a signal
            GlobalEventHandler.Things.OnSentSignal(
                new ThingStateChangedEventArgs(
                    ThingChangeFlag.SentSignal,
                    __instance,
                    __instance.Open ? KnownCompSignals.DoorOpened : KnownCompSignals.DoorClosed));
        }
    }
}