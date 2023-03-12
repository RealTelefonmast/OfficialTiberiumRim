using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [Obsolete]
    public static class MaterialAtlasPool_TR
    {
        private static Dictionary<Material, MaterialAtlas_TR> atlasDict = new Dictionary<Material, MaterialAtlas_TR>();
        private static MethodInfo Create;

        static MaterialAtlasPool_TR()
        {
            Create = AccessTools.Method(AccessTools.TypeByName("MaterialAllocator"), "Create", new[] { typeof(Material) });
        }

        public static Material SubMaterialFromAtlas(Material mat, LinkDirections LinkSet)
        {
            if (!atlasDict.ContainsKey(mat))
            {
                atlasDict.Add(mat, new MaterialAtlas_TR(mat));
            }

            return atlasDict[mat].SubMat(LinkSet);
        }

        private class MaterialAtlas_TR
        {
            protected Material[] subMats = new Material[16];
            private const float TexPadding = 0;//0.03125f;

            public MaterialAtlas_TR(Material newRootMat)
            {
                Vector2 mainTextureScale = new Vector2(0.25f, 0.25f);
                for (int i = 0; i < 16; i++)
                {
                    float x = (float)(i % 4) * 0.25f + TexPadding;
                    float y = (float)(i / 4) * 0.25f + TexPadding;
                    Vector2 mainTextureOffset = new Vector2(x, y);
                    Material material = (Material) Create.Invoke(null, new[] {(object) newRootMat});
                    material.name = newRootMat.name + "_ASMT" + i;
                    material.mainTextureScale = mainTextureScale;
                    material.mainTextureOffset = mainTextureOffset;
                    this.subMats[i] = material;
                }
            }

            public Material SubMat(LinkDirections linkSet)
            {
                if ((int)linkSet >= this.subMats.Length)
                {
                    Log.Warning("Cannot get submat of index " + (int)linkSet + ": out of range.");
                    return BaseContent.BadMat;
                }
                return this.subMats[(int)linkSet];
            }
        }
    }
}
