using System;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public struct SpriteTile
    {
        public Rect rect, normalRect;
        public Vector2 pivot;
        public Material spriteMat;

        public string Label => spriteMat.name;

        public void UpdateRect(Rect parentRect, Rect rect)
        {
            this.rect = rect;
            this.normalRect = new Rect(
                (rect.x / parentRect.width),
                1f - ((rect.y + rect.height) / parentRect.height),
                rect.width / parentRect.width,
                rect.height / parentRect.height);
        }

        public SpriteTile(Rect parentRect, Rect rect, Texture texture)
        {
            this.rect = rect;
            this.normalRect = parentRect;
            this.pivot = Vector2.zero;
            spriteMat = MaterialAllocator.Create(ShaderDatabase.CutoutComplex);

            //
            UpdateRect(parentRect, rect);

            //
            spriteMat.mainTexture = texture;
            spriteMat.name = $"{texture.name}";
            spriteMat.color = Color.white;
        }

        public void DrawTile(Rect rect)
        {
            GenUI.DrawTextureWithMaterial(rect, spriteMat.mainTexture, spriteMat, normalRect);
        }
    }
}
