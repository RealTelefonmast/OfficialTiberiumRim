using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using HarmonyLib;
using RimWorld;
using RimWorld.IO;
using TeleCore.Static;
using TeleCore.Static.Utilities;
//TODO: Resolve TiberiumRim namespace references after consolidation
//using TiberiumRim;
using UnityEngine;
using Verse;

namespace TeleCore;

internal static class InjectPatches
{
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
            foreach (var attackTarg in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackHostile(), true))
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
        static void Postfix(VirtualFile file, ref Texture2D __result)
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