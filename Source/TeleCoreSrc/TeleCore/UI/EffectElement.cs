using TeleCore.Rendering.UI.DynaUI;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace TeleCore.Unsorted;

public class EffectElement : Rendering.UI.DynaUI.UIElement
{
    private readonly Def def;

    public EffectElement(Rect rect, Def def) : base(rect, UIElementMode.Dynamic)
    {
        //
        bgColor = Color.clear;
        hasTopBar = false;

        Size = new Vector2(20, 20);

        //
        this.def = def;
    }

    //DataSet
    public bool IsMote => def is ThingDef;
    public bool IsFleck => def is FleckDef;

    public EffectCanvas ParentCanvas => (EffectCanvas)_parent;

    //EffectData
    public Vector2 EffectOffset => Position - Parent.InRect.center;

    protected override void HandleEvent_Custom(Event ev, bool inContext = false)
    {
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        var label = $"[{def.defName}]\n{EffectOffset}";
        var labelSize = Text.CalcSize(label);
        var labelRect = new Rect(Rect.x, Rect.y - labelSize.y, labelSize.x, labelSize.y);
        TWidgets.DoTinyLabel(labelRect, label);

        //
        GUI.color = Color.red;
        Widgets.DrawTextureFitted(Rect, TeleContent.UIDataNode, 1f);
        GUI.color = Color.white;
    }
}