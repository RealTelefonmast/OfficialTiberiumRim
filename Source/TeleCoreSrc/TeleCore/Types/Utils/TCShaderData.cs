using System.Collections.Generic;
using TeleCore.Types.Structs;

namespace TeleCore.Types.Utils;

internal static class TCShaderData
{
    private static readonly Dictionary<string, ShaderMetaData> shaderDataLookup = new();

    internal static void RegisterShaderData(CustomShaderDef shaderDef)
    {
        shaderDataLookup[shaderDef.shaderInt.name] = new ShaderMetaData
        {
            supportsMask = shaderDef.supportsMask
        };
    }

    public static bool TryGetShaderData(string shaderId, out ShaderMetaData data)
    {
        if (shaderDataLookup.TryGetValue(shaderId, out data)) return true;
        return false;
    }
}