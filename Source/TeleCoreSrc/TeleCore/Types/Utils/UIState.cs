using System.Collections.Generic;
using UnityEngine;

namespace TeleCore.Unsorted;

public struct ElementIdentifier(int id, int uiDepth)
{
    public int Id { get; } = id;

    public int Depth { get; } = uiDepth;
}

public static class UIState
{
    private static int _uiDepth;
    private static readonly HashSet<int> _knownIDs = new();
    private static Dictionary<int, Rendering.UI.DynaUI.UIElement> _elements = new();

    internal static void Begin()
    {
        _uiDepth = 0;
    }

    public static Rendering.UI.DynaUI.UIElement Register(Rect rect)
    {
        var id = GetID(rect);
        var identifier = new ElementIdentifier(id, _uiDepth);
        _uiDepth++;

        //TODO:
        return new Rendering.UI.DynaUI.UIElement();
    }

    private static int GetUniqueID(Rect rect)
    {
        //TODO: Figure out unique ids for overlapping rects
        var id = GetID(rect);
        if (_knownIDs.Contains(id)) return 0;

        return id;
    }

    private static int GetID(Rect rect)
    {
        const float scale = 1000.0f; // Adjust based on your precision needs
        var ix = (int)(rect.x * scale);
        var iy = (int)(rect.y * scale);
        var iw = (int)(rect.width * scale);
        var ih = (int)(rect.height * scale);

        // Combine using bitwise operations
        var hash = 17;
        hash = hash * 31 + ix;
        hash = hash * 31 + iy;
        hash = hash * 31 + iw;
        hash = hash * 31 + ih;

        return hash;
    }
}