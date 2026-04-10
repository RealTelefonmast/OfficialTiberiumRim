using System.Collections.Generic;
using UnityEngine;

namespace TeleCore.UI;

/// <summary>
///     A cachable Rect set, allows you to set rects with a tag and access them via a string tag indexer;
/// </summary>
public struct UILayout
{
    private Dictionary<string, Rect> taggedRects;
    private Vector2 originPos;

    public Rect this[string s]
    {
        get
        {
            var r = taggedRects[s];
            return new Rect(originPos.x + r.x, originPos.y + r.y, r.width, r.height);
        }
    }

    public Rect GetRect(string tag, Vector2 atPos)
    {
        var r = this[tag];
        return new Rect(atPos.x + r.x, atPos.y + r.y, r.width, r.height);
    }

    public void Register(string tag, Rect rect)
    {
        taggedRects ??= new Dictionary<string, Rect>();
        taggedRects.Add(tag, rect);
    }

    public void SetOrigin(Vector2 origin)
    {
        originPos = origin;
    }

    public void Init()
    {
        SetOrigin(Vector2.zero);
    }
}