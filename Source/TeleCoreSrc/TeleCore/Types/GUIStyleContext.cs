using System;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class GUIStyleContext : IDisposable
{
    private readonly TextAnchor _alignment;
    private readonly Font _font;
    private readonly bool _wordWrap;

    public GUIStyleContext(GUIStyle style, bool setTextStyle) : this(style)
    {
        if (setTextStyle)
        {
            Style.font = Text.CurTextFieldStyle.font;
            Style.alignment = Text.CurTextFieldStyle.alignment;
            Style.wordWrap = Text.CurTextFieldStyle.wordWrap;
        }
    }

    public GUIStyleContext(GUIStyle style)
    {
        Style = style;
        _font = style.font;
        _alignment = style.alignment;
        _wordWrap = style.wordWrap;
    }

    public GUIStyle Style { get; }

    public void Dispose()
    {
        Style.font = _font;
        Style.alignment = _alignment;
        Style.wordWrap = _wordWrap;
    }
}