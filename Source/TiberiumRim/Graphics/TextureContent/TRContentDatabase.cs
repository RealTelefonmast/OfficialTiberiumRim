using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class TRContentDatabase
    {
        private static AssetBundle bundleInt;
        private static Dictionary<string, Shader> lookupShades;
        private static Dictionary<string, ComputeShader> lookupComputeShades;
        private static Dictionary<string, Material> lookupMats;

        public static AssetBundle TiberiumBundle
        {
            get
            {
                if (bundleInt == null)
                { 
                    bundleInt = TiberiumRimMod.mod.MainBundle;
                }
                return bundleInt;
            }
        }
        
        //Shaders
        public static readonly Shader TextureBlend = LoadShader("TextureBlend");
        public static readonly Shader FlowMapShader = LoadShader("FlowMapShader");
        public static readonly Shader FlowMapOnBlend = LoadShader("FlowMapOnBlend");

        public static readonly ComputeShader GasGridCompute = LoadComputeShader("GasGridCompute");
        public static readonly ComputeShader GlowFlooderCompute = LoadComputeShader("GlowFlooder");

        //Materials
        //public static readonly Shader AlphaShader = LoadShader("AlphaShader");
        //public static readonly Material AlphaShaderMaterial = LoadMaterial("ShaderMaterial");

        public static ComputeShader LoadComputeShader(string shaderName)
        {
            if (lookupComputeShades == null)
                lookupComputeShades = new Dictionary<string, ComputeShader>();
            if (!lookupShades.ContainsKey(shaderName))
                lookupComputeShades[shaderName] = TiberiumBundle.LoadAsset<ComputeShader>(shaderName);

            ComputeShader shader = lookupComputeShades[shaderName];
            if (shader == null)
            {
                Log.Warning($"Could not load shader '{shaderName}'");
                return null;
            }
            return shader;
        }

        public static Shader LoadShader(string shaderName)
        {
            if (lookupShades == null)
                lookupShades = new Dictionary<string, Shader>();
            if (!lookupShades.ContainsKey(shaderName))
                lookupShades[shaderName] = TiberiumBundle.LoadAsset<Shader>(shaderName);

            Shader shader = lookupShades[shaderName];
            if (shader == null)
            {
                Log.Warning($"Could not load shader '{shaderName}'");
                return ShaderDatabase.DefaultShader;
            }
            return shader;
        }

        public static Material LoadMaterial(string materialName)
        {
            if (lookupMats == null)
                lookupMats = new Dictionary<string, Material>();
            if (!lookupMats.ContainsKey(materialName))
                lookupMats[materialName] = TiberiumBundle.LoadAsset<Material>(materialName);

            Material mat = lookupMats[materialName];
            if (mat == null)
            {
                Log.Warning($"Could not load material '{materialName}'");
                return BaseContent.BadMat;
            }
            return mat;
        }
    }
}
