using UnityEngine;
using Verse;

namespace TeleCore.DDGUI;

public static class Utils
{
    //TODO: Support custom fonts
    public static Vector2 TextSize(string text)
    {
        Text.tmpTextGUIContent.text = text.StripTags();
        var fontStyle = Text.CurFontStyle;
        //IMGUI default style: GUI.skin.label;
        return fontStyle.CalcSize(Text.tmpTextGUIContent);
    }
}