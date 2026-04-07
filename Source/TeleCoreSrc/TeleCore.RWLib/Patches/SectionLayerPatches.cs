using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TeleCore.Mod.Loading;


public static class SectionLayerPatches
{
    [HarmonyPatch(typeof(Printer_Plane))]
    [HarmonyPatch(nameof(Printer_Plane.PrintPlane))]
    public static class Printer_Plane_PrintPlanePatch
    {
        public static bool Prefix(SectionLayer layer, Vector3 center, Vector2 size, Material mat, float rot = 0f,
            bool flipUv = false, Vector2[] uvs = null, Color32[] colors = null, float topVerticesAltitudeBias = 0.01f,
            float uvzPayload = 0f)
        {
            if (layer is not SectionLayer_NetworkGrid networkLayer) return true;
            if (colors == null) colors = Printer_Plane.defaultColors;
            if (uvs == null) uvs = flipUv ? Printer_Plane.defaultUvsFlipped : Printer_Plane.defaultUvs;
            var subMesh = networkLayer.GetSubMeshCurrent(mat);
            var count = subMesh.verts.Count;
            subMesh.verts.Add(new Vector3(-0.5f * size.x, 0f, -0.5f * size.y));
            subMesh.verts.Add(new Vector3(-0.5f * size.x, topVerticesAltitudeBias, 0.5f * size.y));
            subMesh.verts.Add(new Vector3(0.5f * size.x, topVerticesAltitudeBias, 0.5f * size.y));
            subMesh.verts.Add(new Vector3(0.5f * size.x, 0f, -0.5f * size.y));
            if (rot != 0f)
            {
                var num = rot * ((float)Math.PI / 180f);
                num *= -1f;
                for (var i = 0; i < 4; i++)
                {
                    var x = subMesh.verts[count + i].x;
                    var z = subMesh.verts[count + i].z;
                    var num2 = Mathf.Cos(num);
                    var num3 = Mathf.Sin(num);
                    var x2 = x * num2 - z * num3;
                    var z2 = x * num3 + z * num2;
                    subMesh.verts[count + i] = new Vector3(x2, subMesh.verts[count + i].y, z2);
                }
            }

            for (var j = 0; j < 4; j++)
            {
                subMesh.verts[count + j] += center;
                subMesh.uvs.Add(new Vector3(uvs[j].x, uvs[j].y, uvzPayload));
                subMesh.colors.Add(colors[j]);
            }

            subMesh.tris.Add(count);
            subMesh.tris.Add(count + 1);
            subMesh.tris.Add(count + 2);
            subMesh.tris.Add(count);
            subMesh.tris.Add(count + 2);
            subMesh.tris.Add(count + 3);
            return false;
        }
    }
}