using System.Collections.Generic;
using UnityEngine;

namespace TR
{
    public struct SpriteImage
    {
        public string path;
        public PixelData data;
    }

    public class SpriteAtlas
    {
        public List<Texture2D> allSprites = new List<Texture2D>();

        public SpriteAtlas(string path)
        {

        }
    }

    public class PixelData
    {
        public int height;
        public int width;
        public Color[] pixels;

        public PixelData(int width, int height, Color[] pixels)
        {
            this.width = width;
            this.height = height;
            this.pixels = pixels;
        }
    }
}
