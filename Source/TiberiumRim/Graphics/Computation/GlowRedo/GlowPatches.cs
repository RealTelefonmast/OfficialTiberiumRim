using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class GlowPatches
    {
        //Section Layer
        [HarmonyPatch(typeof(SectionLayer_LightingOverlay), "Regenerate")]
        public static class SectionLayer_LightingOverlay_Regenerate_Patch
        {
            public static bool Prefix(SectionLayer_LightingOverlay __instance)
            {
                return true;
                LayerSubMesh subMesh = __instance.GetSubMesh(MatBases.LightOverlay);
                if (subMesh.verts.Count == 0)
                {
                    __instance.MakeBaseGeometry(subMesh);
                }

                //GPU Call
                var array = new Color32[]{};
                subMesh.mesh.colors32 = array;
                return false;
            }

            /*
            public static void Postfix(SectionLayer_LightingOverlay __instance)
            {
                LayerSubMesh subMesh = __instance.GetSubMesh(MatBases.LightOverlay);
                int width = __instance.sectRect.maxX+1 - __instance.sectRect.minX;
                int height = __instance.sectRect.maxZ+1 - __instance.sectRect.minZ;
                //if(__instance.section.botLeft == IntVec3.Zero)
                    //Log.Message($"[{__instance.section.botLeft}] Colors: {subMesh.mesh.colors32.ToStringSafeEnumerable()} | Size: {width} * {height}: {width * height}");
                TiberiumContent.GenerateTextureFrom(subMesh.mesh.colors32, new IntVec2(width, height), $"GlowGrid_Vanilla_{__instance.section.botLeft}");
            }
            */
        }

        //TODO: Redo GlowIO
        //GlowGrid I/O
        [HarmonyPatch(typeof(GlowGrid), "RegisterGlower")]
        public static class GlowGrid_RegisterGlower_Patch
        {
            public static void Postfix(GlowGrid __instance, CompGlower newGlow)
            {
                //__instance.map.Tiberium().GlowGridGPUInfo.Notify_AddGlower(newGlow);
            }
        }

        [HarmonyPatch(typeof(GlowGrid), "DeRegisterGlower")]
        public static class GlowGrid_DeRegisterGlower_Patch
        {
            public static void Postfix(GlowGrid __instance, CompGlower oldGlow)
            {
                //__instance.map.Tiberium().GlowGridGPUInfo.Notify_RemoveGlower(oldGlow);
            }
        }
    }
}
