using System;
using TeleCore.Lib.Factories;
using Unity.Mathematics;
using UnityEngine;
using Verse;

namespace TeleCore.DDGUI;

public enum StringLayoutType : byte
{
    Horizontal,
    Vertical,
    Grid
}

public struct StringLayout
{
    private StringLayoutType _type;
    private int _maxColumns = 0;

    public Vector2 _cellSize;
    public Vector2[] _cellPositions;
    private Rect[] _rects;
 
    private Rect _total;
    private Vector2 _size;
    
    public Vector2 this[int x] =>;
    
    //Horizonal
    public StringLayout(string[] strings, Margin margin = default)
    {
        var max = Vector2.zero;
        var sizes = strings.Factory(Utils.TextSize, Processor);

        _rects = new Rect[sizes.Length];

        float x = margin.Left;
        for (var i = 0; i < sizes.Length; i++)
        {
            _rects[i] = new Rect(x, margin.Top, sizes[i].x, max.y);
            x += sizes[i].x + margin.Right + margin.Left;
        }

        _size = new Vector2(x - margin.Left, max.y + margin.Top + margin.Bottom);

        _total = new Rect(Vector2.zero, _size);
        return;

        void Processor(string s, Vector2 v)
        {
            max.x = math.max(max.x, v.x);
            max.y = math.max(max.y, v.y);
        }
    }

    public StringLayout(string[] strings, StringLayoutType type = StringLayoutType.Horizontal, byte columns = 0)
    {
        _type = type;
        _maxColumns = columns;

        var sizes = strings.Factory(Utils.TextSize);

    }
    
    private static void Grow(StringLayoutType type, Vector2 size, ref Vector2 pos, ref Vector2 totalSize)
    {
        switch (type)
        {
            case StringLayoutType.Horizontal:
                pos.x += size.x;
                totalSize.x += size.x;
                totalSize.y = math.max(pos.y, totalSize.y);
                break;
            case StringLayoutType.Vertical:
                pos.y += size.y;
                totalSize.x = math.max(pos.x, totalSize.x);
                totalSize.y += size.y;
                break;
            case StringLayoutType.Grid:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}