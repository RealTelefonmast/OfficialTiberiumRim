using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace TiberiumRim
{
    public static class MiscPatches
    {
        //
        [HarmonyPatch(typeof(MemoryUtility))]
        [HarmonyPatch(nameof(MemoryUtility.ClearAllMapsAndWorld))]
        internal static class MemoryUtility_ClearAllMapsAndWorldPatch
        {
            public static void Postfix()
            {
                StaticData.Notify_ClearingMapAndWorld();
            }
        }

        //Patching Pref Changes
        [HarmonyPatch(typeof(Prefs))]
        [HarmonyPatch(nameof(Prefs.BackgroundImageExpansion), MethodType.Setter)]
        public static class Prefs_BackgroundImageExpansionSetterPatch
        {
            public static void Postfix(ExpansionDef value)
            {
                TRLog.Debug($"Patching Pref Setter {value}");
                if (value != null)
                {
                    TiberiumSettings.Settings.UseCustomBackground = false;
                    TRLog.Debug("Setting UseCustomBG to FALSE");
                }
            }
        }

        //Patching the vanilla shader def to allow custom shaders
        [HarmonyPatch(typeof(ShaderTypeDef))]
        [HarmonyPatch("Shader", MethodType.Getter)]
        public static class ShaderPatch
        {
            public static bool Prefix(ShaderTypeDef __instance, ref Shader __result, ref Shader ___shaderInt)
            {
                if (__instance is TRShaderTypeDef)
                {
                    if (___shaderInt == null)
                    {
                        ___shaderInt = TRContentDatabase.LoadShader(__instance.shaderPath);
                    }
                    __result = ___shaderInt;
                    return false;
                }
                return true;
            }
        }
    }
}
