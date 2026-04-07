using System.Linq;
using HarmonyLib;
using RimWorld;
using TeleCore.Events;
using TeleCore.Interfaces;
using TeleCore.Static;
using TeleCore.ThingComps.Props;
using TeleCore.Utility;
using UnityEngine;
using Verse;

namespace TeleCore.Patches;

internal static class ThingPatches
{
    private static class Tools
    {
        //TODO: Move to FlowCore
        internal static bool PipeBlocking(ThingDef constr, ThingDef pipe)
        {
            if (constr == null || pipe == null) return false;
            var networkConstr = constr.GetCompProperties<CompProperties_Network>();
            var networkPipe = pipe.GetCompProperties<CompProperties_Network>();
            if (networkConstr == null || networkPipe == null) return false;

            return (from network in networkConstr.networks
                from pipeNetwork in networkPipe.networks
                where network.networkDef == pipeNetwork.networkDef
                select network).Any();
        }

        internal static ThingDef GetThingDef(ThingDef thingDef)
        {
            return thingDef.entityDefToBuild as ThingDef ?? thingDef;
        }

        internal static ThingDef? GetThingDef(Thing thing)
        {
            ThingDef? thingDef;
            if (thing is Blueprint)
                thingDef = thing.def.entityDefToBuild as ThingDef;
            else if (thing is Frame)
                thingDef = thing.def.entityDefToBuild as ThingDef;
            else
                thingDef = thing.def;

            return thingDef;
        }
    }

    [HarmonyPatch(typeof(GenSpawn), nameof(GenSpawn.SpawningWipes))]
    internal static class GenSpawnSpawningWipesPatch
    {
        public static bool Prefix(BuildableDef newEntDef, BuildableDef oldEntDef, ref bool __result)
        {
            if (newEntDef is ThingDef constr && oldEntDef is ThingDef pipe)
                if (Tools.PipeBlocking(Tools.GetThingDef(constr), Tools.GetThingDef(pipe)))
                {
                    __result = true;
                    return false;
                }

            return true;
        }
    }

    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.BlocksConstruction))]
    internal static class GenConstructBlocksConstructionPatch
    {
        internal static bool Prefix(Thing constructible, Thing t, ref bool __result)
        {
            if (t == constructible) return true;
            if (Tools.PipeBlocking(Tools.GetThingDef(constructible), Tools.GetThingDef(t)))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch(nameof(Thing.SpawnSetup))]
    public static class Tele_SpawnSetupPatch
    {
        public static void Postfix(Thing __instance)
        {
            //Event Handling
            GlobalEventHandler.OnThingSpawned(new ThingStateChangedEventArgs(ThingChangeFlag.Spawned, __instance));
        }
    }

    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch(nameof(Thing.DeSpawn))]
    public static class DeSpawnPatch
    {
        public static bool Prefix(Thing __instance)
        {
            //Event Handling
            GlobalEventHandler.OnThingDespawning(new ThingStateChangedEventArgs(ThingChangeFlag.Despawning,
                __instance));
            return true;
        }

        public static void Postfix(Thing __instance)
        {
            //Event Handling
            GlobalEventHandler.OnThingDespawned(new ThingStateChangedEventArgs(ThingChangeFlag.Despawned, __instance));
        }
    }

    [HarmonyPatch(typeof(ThingWithComps))]
    [HarmonyPatch(nameof(ThingWithComps.BroadcastCompSignal))]
    public static class ThingWithComps_BroadcastCompSignalPatch
    {
        public static void Postfix(Thing __instance, string signal)
        {
            //Event Handling
            GlobalEventHandler.OnThingSentSignal(new ThingStateChangedEventArgs(ThingChangeFlag.SentSignal, __instance,
                signal));
        }
    }

    //Door state changed
    [HarmonyPatch(typeof(Building_Door))]
    [HarmonyPatch(nameof(Building_Door.CheckClearReachabilityCacheBecauseOpenedOrClosed))]
    public static class Building_Door_CheckClearReachabilityCacheBecauseOpenedOrClosed_Patch
    {
        public static void Postfix(Building_Door __instance)
        {
            //Event Handling
            GlobalEventHandler.OnThingSentSignal(new ThingStateChangedEventArgs(ThingChangeFlag.SentSignal, __instance,
                __instance.Open ? "DoorOpened" : "DoorClosed"));
        }
    }

    //Projectiles
    [HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch(nameof(Projectile.ArcHeightFactor), MethodType.Getter)]
    public static class ProjectileArcHeightFactorPatch
    {
        public static void Postfix(Projectile __instance, ref float __result)
        {
            if (__instance is IPatchedProjectile patchedProjectile)
                __result += patchedProjectile.ArcHeightFactorPostAdd;
        }
    }

    [HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch(nameof(Projectile.CanHit))]
    public static class ProjectileCanHitPatch
    {
        public static void Postfix(Projectile __instance, Thing thing, ref bool __result)
        {
            if (__instance is IPatchedProjectile patchedProjectile)
                patchedProjectile.CanHitOverride(thing, ref __result);
        }
    }

    [HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch(nameof(Projectile.Launch), typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo),
        typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef))]
    public static class ProjectileLaunchPatch
    {
        public static bool Prefix(Projectile __instance, Thing launcher, Vector3 origin, LocalTargetInfo usedTarget,
            LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false,
            Thing equipment = null, ThingDef targetCoverDef = null)
        {
            //
            if (__instance is IPatchedProjectile patchedProjectile)
                if (!patchedProjectile.PreLaunch(launcher, origin, usedTarget, intendedTarget, hitFlags,
                        preventFriendlyFire, equipment, targetCoverDef))
                    return false;
            return true;
        }

        public static void Postfix(Projectile __instance, Thing launcher, Vector3 origin, LocalTargetInfo usedTarget,
            LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false,
            Thing equipment = null, ThingDef targetCoverDef = null)
        {
            //
            if (__instance is IPatchedProjectile patchedProjectile)
                patchedProjectile.PostLaunch(ref __instance.origin, ref __instance.destination);
        }
    }

    [HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch(nameof(Projectile.ImpactSomething))]
    public static class ProjectileImpactPatch
    {
        private static IntVec3 Position;
        private static Map Map;

        public static bool Prefix(Thing __instance)
        {
            Position = __instance.Position;
            Map = __instance.Map;

            //
            if (__instance is IPatchedProjectile patchedProj) return patchedProj.PreImpact();

            return true;
        }

        public static void Postfix(Thing __instance)
        {
            //
            if (__instance is IPatchedProjectile patchedProj) patchedProj.PostImpact();

            //
            if (__instance.def.HasTeleExtension(out var extension))
                if (extension.projectile != null)
                {
                    extension.projectile.impactExplosion?.DoExplosion(Position, Map, __instance);
                    extension.projectile.impactFilth?.SpawnFilth(Position, Map);
                    extension.projectile.impactEffecter?.Spawn(Position, Map);
                }

            //
            Position = IntVec3.Invalid;
            Map = null;
        }
    }

    [HarmonyPatch(typeof(BuildCopyCommandUtility), nameof(BuildCopyCommandUtility.FindAllowedDesignator))]
    private static class BuildCopyCommandUtility_FindAllowedDesignator_Patch
    {
        private static bool Prefix(BuildableDef buildable, bool mustBeVisible, ref Designator_Build __result)
        {
            if (buildable.HasSubMenuExtension(out var ext))
            {
                var isActive = SubMenuThingDefList.IsActive(ext.ParentDef, buildable);
                if (isActive)
                {
                    __result = GenData.GetDesignatorFor<Designator_Build>(buildable);
                    return false;
                }
            }

            return true;
        }
    }
}