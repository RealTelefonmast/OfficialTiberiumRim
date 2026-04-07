using TeleCore.Utility;
using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator;

public struct SpriteTile
{
    public Rect rect, normalRect;
    public Vector2 pivot;
    public Material spriteMat;

    public string Label => spriteMat.name;

    public void UpdateRect(Rect parentRect, Rect rect)
    {
        this.rect = rect;
        normalRect = TWidgets.RectToTexCoords(parentRect, rect);
    }

    public SpriteTile(Rect parentRect, Rect rect, Texture texture)
    {
        this.rect = rect;
        normalRect = parentRect;
        pivot = Vector2.zero;
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

    public static bool operator ==(SpriteTile tile1, SpriteTile tile2)
    {
        if (tile1.rect != tile2.rect) return false;
        if (tile1.normalRect != tile2.normalRect) return false;
        if (tile1.pivot != tile2.pivot) return false;
        if (tile1.spriteMat != tile2.spriteMat) return false;
        return true;
    }

    public static bool operator !=(SpriteTile tile1, SpriteTile tile2)
    {
        return !(tile1 == tile2);
    }
}