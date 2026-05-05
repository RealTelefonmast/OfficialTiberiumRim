using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.Types.Utils;

/// <summary>
///     Loads custom shaders and materials from the TeleCore.Atmosphere asset bundle.
///     Bundle source is resolved via <see cref="AtmosphereBundle" /> — set this before
///     any content is accessed (e.g. from your mod's Verse.Mod constructor or static init).
/// </summary>
[StaticConstructorOnStartup]
internal static class TAEUnityContent
{
    /// <summary>
    ///     Set this from the consuming mod's entry point to provide the asset bundle.
    /// </summary>
    public static AssetBundle? AtmosphereBundle;

    private static Dictionary<string, Shader> lookupShades;
    private static Dictionary<string, ComputeShader> lookupComputeShades;
    private static Dictionary<string, Material> lookupMats;

    //
    public static ComputeShader GasGridCompute = LoadComputeShader("GasGridCompute");

    //Shaders
    public static readonly Shader TextureBlend = LoadShader("TextureBlend");
    public static readonly Shader CustomOverlay = LoadShader("CustomOverlay");
    public static readonly Shader InstancedGas = LoadShader("InstancedGas");

    public static readonly Material CustomOverlayWorld = LoadMaterial("CustomOverlayWorld");

    public static ComputeShader LoadComputeShader(string shaderName)
    {
        lookupComputeShades ??= new Dictionary<string, ComputeShader>();

        if (AtmosphereBundle != null)
            if (!lookupComputeShades.ContainsKey(shaderName))
                lookupComputeShades[shaderName] = AtmosphereBundle.LoadAsset<ComputeShader>(shaderName);

        if (!lookupComputeShades.TryGetValue(shaderName, out var shader) || shader == null)
        {
            TLog.Warning($"Could not load shader '{shaderName}'");
            return null;
        }

        return shader;
    }

    public static Shader LoadShader(string shaderName)
    {
        lookupShades ??= new Dictionary<string, Shader>();

        if (AtmosphereBundle != null)
            if (!lookupShades.ContainsKey(shaderName))
                lookupShades[shaderName] = AtmosphereBundle.LoadAsset<Shader>(shaderName);

        if (!lookupShades.TryGetValue(shaderName, out var shader) || shader == null)
        {
            TLog.Warning($"Could not load shader '{shaderName}'");
            return ShaderDatabase.DefaultShader;
        }

        return shader;
    }

    public static Material LoadMaterial(string materialName)
    {
        lookupMats ??= new Dictionary<string, Material>();

        if (AtmosphereBundle != null)
            if (!lookupMats.ContainsKey(materialName))
                lookupMats[materialName] = AtmosphereBundle.LoadAsset<Material>(materialName);

        if (!lookupMats.TryGetValue(materialName, out var mat) || mat == null)
        {
            Log.Warning($"Could not load material '{materialName}'");
            return BaseContent.BadMat;
        }

        return mat;
    }
}