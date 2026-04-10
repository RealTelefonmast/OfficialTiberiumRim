using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using HarmonyLib;
using RimWorld;
using RimWorld.IO;
using TeleCore.Hediffs;
using TeleCore.Static;
using TeleCore.Types;
using TeleCore.Utils;
using UnityEngine;
using Verse;

//TODO: Resolve TiberiumRim namespace references after consolidation
//using TiberiumRim;

namespace TeleCore.Mod.Patches;

internal static class InjectPatches
{
    //Allows custom shaders to be loaded
    [HarmonyPatch(typeof(ShaderDatabase), nameof(ShaderDatabase.LoadShader))]
    internal static class LoadShaderPatch
    {
        private static bool Prefix(string shaderPath, ref Shader __result)
        {
            var vanilla = (Shader)Resources.Load("Materials/" + shaderPath, typeof(Shader));
            if (vanilla == null)
            {
                __result = TeleContentDB.LoadShader(shaderPath);
                return false;
            }
            return true;
        }
    }

    //This adds gizmos to the pawn
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("GetGizmos")]
    internal static class Pawn_GetGizmoPatch
    {
        private static void Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
        {
            var gizmos = new List<Gizmo>(__result);

            foreach (var hediff in __instance.health.hediffSet.hediffs)
            {
                if (hediff is HediffWithGizmos gizmoDiff) gizmos.AddRange(gizmoDiff.GetGizmos());
                var gizmoComp = hediff.TryGetComp<HediffComp_Gizmo>();
                if (gizmoComp != null)
                    gizmos.AddRange(gizmoComp.GetGizmos());
            }

            __result = gizmos;
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch("AddDraftedOrders")]
    public static class Pawn_AddDraftedOrdersPatch
    {
        public static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (!pawn.PawnHasRangedHediffVerb()) return;
            foreach (var attackTarg in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackAny(), true))
            {
                var rangedAct = HediffRangedUtility.GetRangedAttackAction(pawn, attackTarg, out var str);
                string text = "FireAt".Translate(attackTarg.Thing.Label, attackTarg.Thing);
                var floatMenuOption = new FloatMenuOption("", null, MenuOptionPriority.High, null, attackTarg.Thing);
                if (rangedAct == null)
                {
                    text += ": " + str;
                }
                else
                {
                    floatMenuOption.autoTakeable = attackTarg.HasThing || attackTarg.Thing.HostileTo(Faction.OfPlayer);
                    floatMenuOption.autoTakeablePriority = 40f;
                    floatMenuOption.action = delegate
                    {
                        FleckMaker.Static(attackTarg.Thing.DrawPos, attackTarg.Thing.Map, FleckDefOf.FeedbackShoot);
                        rangedAct();
                    };
                }

                floatMenuOption.Label = text;
                opts.Add(floatMenuOption);
            }
        }
    }

    //
    [HarmonyPatch]
    public static class EditablePostLoadPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodInfo> TargetMethods()
        {
            // yield return AccessTools.Method(typeof(Def), "PostLoad");
            // yield return AccessTools.Method(typeof(PawnKindDef), "PostLoad");
            // yield return AccessTools.Method(typeof(ThingStyleDef), "PostLoad");
            // yield return AccessTools.Method(typeof(BodyPartDef), "PostLoad");
            // yield return AccessTools.Method(typeof(FactionDef), "PostLoad");
            // yield return AccessTools.Method(typeof(ThingCategoryDef), "PostLoad");
            //
            // yield return AccessTools.Method(typeof(SkillDef), "PostLoad");
            // yield return AccessTools.Method(typeof(AbilityDef), "PostLoad");
            // yield return AccessTools.Method(typeof(MechWorkModeDef), "PostLoad");

            foreach (var type in typeof(Def).AllSubclasses().Concat(typeof(Def)))
                yield return AccessTools.Method(type, nameof(Def.PostLoad));
        }

        public static void Postfix(Def __instance)
        {
            DefIDStack.RegisterNew(__instance);
        }
    }

    //Patching the vanilla shader def to allow custom shaders
    /*[HarmonyPatch(typeof(ShaderTypeDef))]
    [HarmonyPatch("Shader", MethodType.Getter)]
    public static class ShaderPatch
    {
        public static bool Prefix(ShaderTypeDef __instance, ref Shader __result, ref Shader ___shaderInt)
        {
            if (__instance is not CustomShaderDef) return true;

            if (___shaderInt == null)
            {
                ___shaderInt = TeleContentDB.LoadShader(__instance.shaderPath);
            }
            __result = ___shaderInt;
            return false;
        }
    }*/

    [HarmonyPatch(typeof(ModContentLoader<Texture2D>))]
    [HarmonyPatch("LoadTexture")]
    private static class LoadPNGPatch
    {
        private static void Postfix(VirtualFile file, ref Texture2D __result)
        {
            if (__result != null)
            {
                var metaFile = Path.ChangeExtension(file.FullPath, ".xml");
                if (File.Exists(metaFile))
                {
                    var serializer = new XmlSerializer(typeof(TextureMeta));
                    using var stream = new FileStream(metaFile, FileMode.Open);
                    var meta = (TextureMeta)serializer.Deserialize(stream);
                    if (meta != null)
                    {
                        var copy = TextureUtils.CopyReadable(__result);
                        copy.wrapMode = meta.WrapMode;
                        copy.Apply(true, true);
                        __result = copy;
                    }
                }
            }
        }
    }
}