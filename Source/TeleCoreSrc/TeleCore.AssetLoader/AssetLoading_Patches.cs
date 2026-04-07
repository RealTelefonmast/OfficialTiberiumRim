using HarmonyLib;
using TeleCore.Loader;
using UnityEngine;
using Verse;

namespace TeleCore.AssetLoader;

internal static class AssetLoading_Patches
{
    [HarmonyPatch(typeof(ShaderDatabase), nameof(ShaderDatabase.LoadShader))]
    internal static class ShaderDatabase_LoadShaderPatch
    {
        private static bool Prefix(string shaderPath, ref Shader __result)
        {
            //TLog.Debug("Loading shader from database");
            var vanilla = (Shader)Resources.Load("Materials/" + shaderPath, typeof(Shader));
            if (vanilla == null)
            {
                __result = AssetBundleDB.LoadShader(shaderPath);
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(ShaderTypeDef), nameof(ShaderTypeDef.Shader), MethodType.Getter)]
    internal static class ShaderTypeDef_ShaderPatch
    {
        private static void Postfix(ShaderTypeDef __instance)
        {
            if (__instance is CustomShaderDef custom)
            {
                TLog.Debug("Registering shader data: " + __instance.defName);
                TCShaderData.RegisterShaderData(custom);
            }
        }
    }
    
    [HarmonyPatch(typeof(ShaderUtility), nameof(ShaderUtility.SupportsMaskTex))]
    internal static class ShaderUtility_SupportsMaskTexPatch
    {
        private static void Postfix(Shader shader, ref bool __result)
        {
            if (TCShaderData.TryGetShaderData(shader.name, out var meta))
            {
                __result = meta.supportsMask;
            }
        }
    }
}