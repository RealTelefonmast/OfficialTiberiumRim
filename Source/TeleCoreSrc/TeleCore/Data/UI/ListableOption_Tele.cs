using System;
using TeleCore.Static;
using UnityEngine;
using Verse;

namespace TeleCore;

public class ListableOption_Tele : ListableOption
{
    public ListableOption_Tele(string label, Action action, string uiHighlightTag = null) : base(label, action,
        uiHighlightTag)
    {
    }

    public override float DrawOption(Vector2 pos, float width)
    {
        var b = Text.CalcHeight(label, width);
        var num = Mathf.Max(minHeight, b);
        var rect = new Rect(pos.x, pos.y, width, num);

        var atlas = TeleContent.ButtonBGAtlas;
        if (Mouse.IsOver(rect))
        {
            atlas = TeleContent.ButtonBGAtlasMouseover;
            if (Input.GetMouseButton(0)) atlas = TeleContent.ButtonBGAtlasClick;
        }

        Widgets.DrawAtlas(rect, atlas);

        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(rect, label);
        Text.Anchor = default;


        if (Widgets.ButtonInvisible(rect))
            action();

        if (uiHighlightTag != null)
            UIHighlighter.HighlightOpportunity(rect, uiHighlightTag);
        return num;
    }
}