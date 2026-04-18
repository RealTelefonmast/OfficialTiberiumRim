using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

internal static class VerbPatches
{
    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras))]
    internal static class PawnRenderer_DrawEquipmentAndApparelExtrasPatch
    {
        private static void Postfix(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
        {
            if (pawn?.CurrentEffectiveVerb is Verb_Tele teleVerb)
                teleVerb.DrawVerb(drawPos);
        }
    }

    [HarmonyPatch(typeof(Verb), nameof(Verb.VerbTick))]
    internal static class Verb_VerbTickPatch
    {
        private static bool Prefix(Verb __instance)
        {
            if (__instance is Verb_Tele teleVerb) teleVerb.PreVerbTick();
            return true;
        }

        private static void Postfix(Verb __instance)
        {
            if (__instance is Verb_Tele teleVerb) teleVerb.PostVerbTick();
        }
    }

    [HarmonyPatch(typeof(VerbTracker), nameof(VerbTracker.CreateVerbTargetCommand))]
    internal static class CreateVerbTargetCommandPatch
    {
        private static void Postfix(Command_VerbTarget __result, Verb verb)
        {
            if (!verb.Available())
                __result.Disable("Not available...");
        }
    }

    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.LaunchesProjectile), MethodType.Getter)]
    internal static class VerbPropertiesLaunchesProjectilePatch
    {
        private static bool Prefix(VerbProperties __instance, ref bool __result)
        {
            if (__instance is VerbProperties_Extended teleProps)
            {
                __result = teleProps.isProjectile;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(VerbUtility), nameof(VerbUtility.GetProjectile))]
    internal static class VerbUtilityGetProjectilePatch
    {
        private static bool Prefix(Verb verb, ref ThingDef __result)
        {
            if (!VerbWatcher.HasAttacher(verb)) return true;
            var attacher = VerbWatcher.GetAttacher(verb);
            __result = attacher?.Projectile ?? __result;
            return false;
        }
    }

    [HarmonyPatch(typeof(VerbUtility), nameof(VerbUtility.GetDamageDef))]
    internal static class VerbUtilityGetDamageDefPatch
    {
        private static bool Prefix(Verb verb, ref DamageDef __result)
        {
            if (verb is Verb_Tele verbTele)
            {
                __result = verbTele.DamageDef;
                return false;
            }

            return true;
        }
    }

    #region Verb Attacher / Watcher

    [HarmonyPatch(typeof(VerbTracker), nameof(VerbTracker.InitVerb))]
    internal static class VerbTracker_InitVerb_Patch
    {
        private static void Postfix(Verb verb, VerbTracker __instance)
        {
            VerbWatcher.Notify_NewVerb(verb, __instance);
        }
    }

    // ## WarmUp ##
    [HarmonyPatch(typeof(Verb), nameof(Verb.WarmupComplete))]
    internal static class Verb_WarmupComplete_Patch
    {
        private static bool Prefix(Verb __instance)
        {
            VerbWatcher.GetAttacher(__instance)?.Notify_WarmupComplete();
            return true;
        }
    }

    // ## LaunchProjectile ##

    [HarmonyPatch(typeof(Verb_LaunchProjectile), nameof(Verb_LaunchProjectile.TryCastShot))]
    internal static class VerbLaunchProjectile_TryCastShot_Patch
    {
        internal static Projectile _projectile;
        internal static TeleVerbAttacher _attacher;

        private static readonly MethodInfo ProjectileLaunch = AccessTools.Method(typeof(Projectile),
            nameof(Projectile.Launch),
            [
                typeof(Thing),
                typeof(Vector3),
                typeof(LocalTargetInfo),
                typeof(LocalTargetInfo),
                typeof(ProjectileHitFlags),
                typeof(bool),
                typeof(Thing),
                typeof(ThingDef)
            ]);

        private static readonly MethodInfo TransformCastOriginMethod = AccessTools.Method(typeof(VerbLaunchProjectile_TryCastShot_Patch), nameof(TransformCastOrigin));
        private static readonly FieldInfo CachedProjectileFld = AccessTools.Field(typeof(VerbLaunchProjectile_TryCastShot_Patch), nameof(_projectile));

        [HarmonyPrefix]
        internal static bool Prefix(Verb_LaunchProjectile __instance)
        {
            _attacher = VerbWatcher.GetAttacher(__instance);
            return true;
        }

        [HarmonyPostfix]
        internal static void Postfix(Verb_LaunchProjectile __instance)
        {
            GlobalRoomEventHandler.OnProjectileLaunched(new ProjectileLaunchedArgs(_projectile));
            _attacher.Notify_ProjectileLaunched(_projectile);
            _projectile = null;
            _attacher = null;
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (TryInjectProjectileLaunchPosition(instruction, out var sub1))
                {
                    foreach (var subInstruction in sub1)
                    {
                        yield return subInstruction;
                    }
                    continue;
                }

                if (TryInjectCacheLaunchedProjectile(instruction, out var sub2))
                {
                    foreach (var subInstruction in sub2)
                    {
                        yield return subInstruction;
                    }
                    continue;
                }

                yield return instruction;
            }
        }

        private static bool TryInjectProjectileLaunchPosition(CodeInstruction instruction, out IEnumerable<CodeInstruction> instructions)
        {
            instructions = Enumerable.Empty<CodeInstruction>();
            if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder { LocalIndex: 6 })
            {
                instructions = new[]
                {
                    new CodeInstruction(OpCodes.Call, TransformCastOriginMethod),
                    instruction
                };
                return true;
            }
            return false;
        }

        private static bool TryInjectCacheLaunchedProjectile(CodeInstruction instruction, out IEnumerable<CodeInstruction> instructions)
        {
            instructions = Enumerable.Empty<CodeInstruction>();
            if (instruction.Calls(ProjectileLaunch))
            {
                instructions = new[]
                {
                    instruction,
                    new CodeInstruction(OpCodes.Ldloc_S, 7),
                    new CodeInstruction(OpCodes.Stsfld, CachedProjectileFld)
                };
                return true;
            }
            return false;
        }

        private static Vector3 TransformCastOrigin(Vector3 initial)
        {
            return _attacher?.GetLaunchPosition(initial) ?? initial;
        }
    }

    #endregion

    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras))]
    internal static class PawnRenderer_DrawEquipmentAndApparelExtrasPatch
    {
        private static void Postfix(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
        {
            if (pawn?.CurrentEffectiveVerb is Verb_Tele teleVerb)
                teleVerb.DrawVerb(drawPos);
        }
    }

    [HarmonyPatch(typeof(Verb), nameof(Verb.VerbTick))]
    internal static class Verb_VerbTickPatch
    {
        private static bool Prefix(Verb __instance)
        {
            if (__instance is Verb_Tele teleVerb) teleVerb.PreVerbTick();
            return true;
        }

        private static void Postfix(Verb __instance)
        {
            if (__instance is Verb_Tele teleVerb) teleVerb.PostVerbTick();
        }
    }

    [HarmonyPatch(typeof(VerbTracker), nameof(VerbTracker.CreateVerbTargetCommand))]
    internal static class CreateVerbTargetCommandPatch
    {
        private static void Postfix(Command_VerbTarget __result, Verb verb)
        {
            if (!verb.Available())
                __result.Disable("Not available...");
        }
    }

    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.LaunchesProjectile), MethodType.Getter)]
    internal static class VerbPropertiesLaunchesProjectilePatch
    {
        private static bool Prefix(VerbProperties __instance, ref bool __result)
        {
            if (__instance is VerbProperties_Extended teleProps)
            {
                __result = teleProps.isProjectile;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(VerbUtility), nameof(VerbUtility.GetProjectile))]
    internal static class VerbUtilityGetProjectilePatch
    {
        private static bool Prefix(Verb verb, ref ThingDef __result)
        {
            if (!VerbWatcher.HasAttacher(verb)) return true;
            var attacher = VerbWatcher.GetAttacher(verb);
            __result = attacher?.Projectile ?? __result;
            return false;
        }
    }

    [HarmonyPatch(typeof(VerbUtility), nameof(VerbUtility.GetDamageDef))]
    internal static class VerbUtilityGetDamageDefPatch
    {
        private static bool Prefix(Verb verb, ref DamageDef __result)
        {
            if (verb is Verb_Tele verbTele)
            {
                __result = verbTele.DamageDef;
                return false;
            }

            return true;
        }
    }
}