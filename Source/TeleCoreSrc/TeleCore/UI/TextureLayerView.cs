using UnityEngine;

namespace TeleCore.UI;

internal class TextureLayerView : Rendering.UI.DynaUI.UIElement
{
    private readonly ElementScroller internalScroller;
    private Rendering.UI.DynaUI.UIElement parentContainer;

    public TextureLayerView(Rendering.UI.DynaUI.UIElement parentContainer) : base(UIElementMode.Static)
    {
        this.parentContainer = parentContainer;
        internalScroller = new ElementScroller(parentContainer, UIElementMode.Static);
    }

    public TextureElement ActiveElement => internalScroller.SelectedElement as TextureElement;

    public void Notify_SelectIndex(int index)
    {
        internalScroller.Notify_SelectIndex(index);
    }

    protected override void HandleEvent_Custom(Event ev, bool inContext)
    {
        base.HandleEvent_Custom(ev);
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        var rect = new Rect(inRect.x - 1, inRect.y, inRect.width + 2, inRect.height);
        internalScroller.DrawElement(rect);
    }
}