using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.Defs;

public struct GUIRectOffset
{
    public int left;
    public int right;
    public int top;
    public int bottom;

    public RectOffset RectOffset => new(left, right, top, bottom);
}

public enum GUIStyleType
{
    Button,
    TextField
}

public class GUIStyleDef : Def
{
    public GUIStyleStateDef active;
    public TextAnchor alignment = TextAnchor.UpperLeft;
    public GUIRectOffset? border;
    public TextClipping clipping = TextClipping.Overflow;
    public Vector2 contentOffset = Vector2.zero;
    public float fixedHeight = 0;
    public float fixedWidth = 0;
    public GUIStyleStateDef focused;

    public FontDef? font;
    public int fontSize = 0;
    public FontStyle fontStyle = FontStyle.Normal;
    public GUIStyleStateDef hover;
    public ImagePosition imagePosition = ImagePosition.ImageLeft;
    public GUIRectOffset? margin;
    public GUIStyleStateDef normal;
    public GUIStyleStateDef onActive;
    public GUIStyleStateDef onFocused;
    public GUIStyleStateDef onHover;
    public GUIStyleStateDef onNormal;
    public GUIRectOffset? overflow;
    public GUIRectOffset? padding;
    public bool richText = false;
    public bool stretchHeight = false;
    public bool stretchWidth = false;
    public GUIStyleDef? styleDefToInherit;
    public GUIStyleType? styleTypeToInherit;
    public bool wordWrap = false;


    public GUIStyle GetStyle()
    {
        GUIStyle? style = null;
        if (styleTypeToInherit != null)
            style = styleTypeToInherit switch
            {
                GUIStyleType.Button => new GUIStyle(GUI.skin.button),
                GUIStyleType.TextField => new GUIStyle(GUI.skin.textField),
                _ => style
            };

        if (styleDefToInherit != null)
            style = styleDefToInherit.GetStyle();

        style ??= new GUIStyle();
        style.font = font.Font;
        style.imagePosition = imagePosition;
        style.alignment = alignment;
        style.wordWrap = wordWrap;
        style.clipping = clipping;
        style.contentOffset = contentOffset;
        style.fixedWidth = fixedWidth;
        style.fixedHeight = fixedHeight;
        style.stretchWidth = stretchWidth;
        style.stretchHeight = stretchHeight;
        style.fontSize = fontSize;
        style.fontStyle = fontStyle;
        style.richText = richText;
        style.name = defName;
        style.normal = normal.State;
        style.hover = hover.State;
        style.active = active.State;
        style.onNormal = onNormal.State;
        style.onHover = onHover.State;
        style.onActive = onActive.State;
        style.focused = focused.State;
        style.onFocused = onFocused.State;
        style.border = border?.RectOffset ?? null;
        style.margin = margin?.RectOffset ?? null;
        style.padding = padding?.RectOffset ?? null;
        style.overflow = overflow?.RectOffset ?? null;
        return style;
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors())
            yield return error;

        if (styleTypeToInherit != null && styleDefToInherit == null)
            yield return "Can only inherit from one style type. (internal type or GUIStyleDef)";
    }

    public override void PostLoad()
    {
        base.PostLoad();
        var state = new GUIStyleState();
        var style = new GUIStyle();

        state.background = null;
        state.textColor = Color.white;
    }
}