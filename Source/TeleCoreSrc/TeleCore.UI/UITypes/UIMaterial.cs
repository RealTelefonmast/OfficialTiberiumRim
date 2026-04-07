using UnityEngine;

namespace TeleCore.TeleUI.UITypes;

public struct UIMaterial
{
    public Texture2D Texture { get; }
    public Material Material { get; }
        
    public UIMaterial(Texture2D texture, Material material)
    {
        Texture = texture;
        Material = material;
    }
}