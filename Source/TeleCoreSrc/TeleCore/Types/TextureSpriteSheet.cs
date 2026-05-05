using System.Collections.Generic;
using TeleCore.Types.Structs;
using UnityEngine;

namespace TeleCore.Types;

public class TextureSpriteSheet
{
    private string name;

    public TextureSpriteSheet(Texture texture, List<SpriteTile> tiles)
    {
        Texture = texture;
        Tiles = tiles;
    }

    public Texture Texture { get; }

    public List<SpriteTile> Tiles { get; }

    public void DrawData(Rect rect)
    {
    }
}