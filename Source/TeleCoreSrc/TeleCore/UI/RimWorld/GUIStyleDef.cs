using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.UI;

public struct GUIRectOffset
{
    public int left;
    public int right;
    public int top;
    public int bottom;
    
    public RectOffset RectOffset => new RectOffset(left, right, top, bottom);
}

public enum GUIStyleType
{
    Button,
    TextField
}

public class GUIStyleDef : Def
{
    public GUIStyleType? styleTypeToInherit;
    public GUIStyleDef? styleDefToInherit;
    
    public FontDef? font;
    public ImagePosition imagePosition = ImagePosition.ImageLeft;
    public TextAnchor alignment = TextAnchor.UpperLeft;
    public bool wordWrap = false;
    public TextClipping clipping = TextClipping.Overflow;
    public Vector2 contentOffset = Vector2.zero;
    public float fixedWidth = 0;
    public float fixedHeight = 0;
    public bool stretchWidth = false;
    public bool stretchHeight = false;
    public int fontSize = 0;
    public FontStyle fontStyle = FontStyle.Normal;
    public bool richText = false;
    public GUIStyleStateDef normal;
    public GUIStyleStateDef hover;
    public GUIStyleStateDef active;
    public GUIStyleStateDef onNormal;
    public GUIStyleStateDef onHover;
    public GUIStyleStateDef onActive;
    public GUIStyleStateDef focused;
    public GUIStyleStateDef onFocused;
    public GUIRectOffset? border;
    public GUIRectOffset? margin;
    public GUIRectOffset? padding;
    public GUIRectOffset? overflow;
    

    public GUIStyle GetStyle()
    {
        GUIStyle? style = null;
        if (styleTypeToInherit != null)
        {
            style = styleTypeToInherit switch
            {
                GUIStyleType.Button => new GUIStyle(GUI.skin.button),
                GUIStyleType.TextField => new GUIStyle(GUI.skin.textField),
                _ => style
            };
        }
        
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
        foreach(var error in base.ConfigErrors())
            yield return error;
        
        if(styleTypeToInherit != null && styleDefToInherit == null)
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