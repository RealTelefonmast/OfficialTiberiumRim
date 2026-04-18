using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class EffectCanvas : UIElement, IDragAndDropReceiver
{
    private ThingDef _currentHolder;

    public EffectCanvas(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
    {
        UIDragNDropper.RegisterAcceptor(this);
    }

    public EffectCanvas(UIElementMode mode) : base(mode)
    {
        UIDragNDropper.RegisterAcceptor(this);
    }

    //
    public bool TryAcceptDrop(object draggedObject, Vector2 pos)
    {
        if (draggedObject is ThingDef effectHolder && (effectHolder.IsBuilding() ||
                                                       effectHolder.category is ThingCategory.Item
                                                           or ThingCategory.Building))
        {
            _currentHolder = effectHolder;
            return true;
        }

        if (draggedObject is ThingDef {mote: not null} effectDef)
        {
            var element = new EffectElement(new Rect(pos, new Vector2(20, 20)), effectDef);
            AddElement(element);
        }

        return false;
    }

    public bool CanAcceptDrop(object draggedObject)
    {
        return draggedObject is Def;
    }

    public void DrawHoveredData(object draggedObject, Vector2 pos)
    {
        if (draggedObject is Def def)
        {
            var texture = TWidgets.TextureForFleckMote(def);
            var labelSize = Text.CalcSize(def.defName);
            var drawRect = pos.RectOnPos(new Vector2(20, 20));
            var labelRect = new Rect(drawRect.x, drawRect.y - 20, labelSize.x, labelSize.y);
            TWidgets.DoTinyLabel(labelRect, $"[{def.defName}]");

            Verse.Widgets.DrawTextureFitted(drawRect, TeleContent.UIDataNode, 1f);
        }
    }

    protected override IEnumerable<FloatMenuOption> RightClickOptions()
    {
        foreach (var rightClickOption in base.RightClickOptions()) yield return rightClickOption;
    }

    protected override void DrawTopBarExtras(Rect topRect)
    {
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        base.DrawContentsBeforeRelations(inRect);
        if (_currentHolder != null) //Draw Effect Holder Thing
        {
            GUI.color = TColor.White05;
            Verse.Widgets.DrawTextureFitted(inRect, _currentHolder.ToTextureAndColor().Texture, 1f);
            GUI.color = Color.white;
        }
    }
}