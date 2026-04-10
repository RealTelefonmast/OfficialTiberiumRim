using System;
using UnityEngine;
using Verse;

namespace TeleCore.UI;

public class GUIStyleContext : IDisposable
{
    private GUIStyle _style;
    private Font _font;
    private TextAnchor _alignment;
    private bool _wordWrap;

    public GUIStyle Style => _style;

    public GUIStyleContext(GUIStyle style, bool setTextStyle) : this(style)
    {
        if (setTextStyle)
        {
            _style.font = Text.CurTextFieldStyle.font;
            _style.alignment = Text.CurTextFieldStyle.alignment;
            _style.wordWrap = Text.CurTextFieldStyle.wordWrap;
        }
    }

    public GUIStyleContext(GUIStyle style) 
    {
        _style = style;
        _font = style.font;
        _alignment = style.alignment;
        _wordWrap = style.wordWrap;
    }
    
    public void Dispose()
    {
        _style.font = _font;
        _style.alignment = _alignment;
        _style.wordWrap = _wordWrap;
    }
}
