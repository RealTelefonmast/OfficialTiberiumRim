using System.Collections.Generic;
using UnityEngine;

namespace TeleCore.Rendering.Sprites;

public struct SpriteImage
{
    public string path;
    public PixelData data;
}

public class SpriteAtlas
{
    public List<Texture2D> allSprites = new();

    public SpriteAtlas(string path)
    {
    }
}

public class PixelData
{
    public int height;
    public Color[] pixels;
    public int width;

    public PixelData(int width, int height, Color[] pixels)
    {
        this.width = width;
        this.height = height;
        this.pixels = pixels;
    }
}