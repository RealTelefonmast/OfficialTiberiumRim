using UnityEngine;

namespace TeleCore.UI.DynaUI.Editing;

public class CanvasElement : UIElement
{
    private BaseCanvas _parentCanvas;

    public CanvasElement(UIElementMode mode) : base(mode)
    {
    }

    public CanvasElement(Rect rect, UIElementMode mode) : base(rect, mode)
    {
    }

    public CanvasElement(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
    {
    }

    //TODO use TRS of base class to set element TRS directly as required by canvas => No extra canvas layer simulation

    protected override void Notify_AddedToParent(UIElement parent)
    {
        base.Notify_AddedToParent(parent);
        if (parent is BaseCanvas canvas) _parentCanvas = canvas;
    }
}