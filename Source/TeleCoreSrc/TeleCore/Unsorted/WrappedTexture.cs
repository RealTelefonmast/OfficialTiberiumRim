using UnityEngine;

namespace TeleCore.Unsorted;

public struct WrappedTexture
{
    public string Path { get; private set; }

    public Texture Texture { get; private set; }

    //
    public bool IsValid => Path != null && Texture != null;

    public WrappedTexture(string path, Texture texture)
    {
        this.Path = path;
        this.Texture = texture;
    }

    public void Clear()
    {
        Path = null;
        Texture = null;
    }
}