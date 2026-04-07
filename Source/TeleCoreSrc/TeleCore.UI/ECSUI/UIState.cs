using System.Collections.Generic;
using UnityEngine;

namespace TeleCore.TeleUI.ECSUI;

public struct ElementIdentifier(int id, int uiDepth)
{
    private int _id = id; //Id from Rect
    private int _depth = uiDepth; //Call depth

    public int Id => _id;
    public int Depth => _depth;
}

public static class UIState
{
    private static int _uiDepth = 0;
    private static HashSet<int> _knownIDs = new();
    private static Dictionary<int, UIElement> _elements = new();

    internal static void Begin()
    {
        _uiDepth = 0;
    }
    
    public static UIElement Register(Rect rect)
    {
        var id = GetID(rect);
        var identifier = new ElementIdentifier(id, _uiDepth);
        _uiDepth++;

        //TODO:
        return new UIElement();

    }

    private static int GetUniqueID(Rect rect)
    {
        //TODO: Figure out unique ids for overlapping rects
        var id = GetID(rect);
        if (_knownIDs.Contains(id))
        {
            return 0;
        }

        return id;
    }

    private static int GetID(Rect rect)
    {
        const float scale = 1000.0f; // Adjust based on your precision needs
        int ix = (int)(rect.x * scale);
        int iy = (int)(rect.y * scale);
        int iw = (int)(rect.width * scale);
        int ih = (int)(rect.height * scale);

        // Combine using bitwise operations
        int hash = 17;
        hash = (hash * 31) + ix;
        hash = (hash * 31) + iy;
        hash = (hash * 31) + iw;
        hash = (hash * 31) + ih;

        return hash;
    }
}