using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TR;

[Obsolete]
public static class MaterialAtlasPool_TR
{
    private static readonly Dictionary<Material, MaterialAtlas_TR> atlasDict = new();
    private static readonly MethodInfo Create;

    static MaterialAtlasPool_TR()
    {
        Create = AccessTools.Method(AccessTools.TypeByName("MaterialAllocator"), "Create", new[] { typeof(Material) });
    }

    public static Material SubMaterialFromAtlas(Material mat, LinkDirections LinkSet)
    {
        if (!atlasDict.ContainsKey(mat)) atlasDict.Add(mat, new MaterialAtlas_TR(mat));

        return atlasDict[mat].SubMat(LinkSet);
    }

    private class MaterialAtlas_TR
    {
        private const float TexPadding = 0; //0.03125f;
        protected readonly Material[] subMats = new Material[16];

        public MaterialAtlas_TR(Material newRootMat)
        {
            var mainTextureScale = new Vector2(0.25f, 0.25f);
            for (var i = 0; i < 16; i++)
            {
                var x = i % 4 * 0.25f + TexPadding;
                var y = i / 4 * 0.25f + TexPadding;
                var mainTextureOffset = new Vector2(x, y);
                var material = (Material)Create.Invoke(null, new[] { (object)newRootMat });
                material.name = newRootMat.name + "_ASMT" + i;
                material.mainTextureScale = mainTextureScale;
                material.mainTextureOffset = mainTextureOffset;
                subMats[i] = material;
            }
        }

        public Material SubMat(LinkDirections linkSet)
        {
            if ((int)linkSet >= subMats.Length)
            {
                Log.Warning("Cannot get submat of index " + (int)linkSet + ": out of range.");
                return BaseContent.BadMat;
            }

            return subMats[(int)linkSet];
        }
    }
}