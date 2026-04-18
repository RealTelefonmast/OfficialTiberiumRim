using System.Collections.Generic;

namespace TeleCore.Unsorted;

internal static class TCShaderData
{
    private static Dictionary<string, ShaderMetaData> shaderDataLookup = new Dictionary<string, ShaderMetaData>();
    
    internal static void RegisterShaderData(CustomShaderDef shaderDef)
    {
        shaderDataLookup[shaderDef.shaderInt.name] = new ShaderMetaData
        {
            supportsMask = shaderDef.supportsMask
        };
    }
    
    public static bool TryGetShaderData(string shaderId, out ShaderMetaData data)
    {
        if (shaderDataLookup.TryGetValue(shaderId, out data))
        {
            return true;
        }
        return false;
    }
}