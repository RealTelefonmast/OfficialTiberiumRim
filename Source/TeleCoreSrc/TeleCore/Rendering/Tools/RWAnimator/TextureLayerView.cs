using TeleCore.UI.DynaUI;
using TeleCore.UI.DynaUI.Editing;
using UnityEngine;

namespace TeleCore.Rendering.Tools.RWAnimator;

internal class TextureLayerView : UIElement
{
    private readonly ElementScroller internalScroller;
    private UIElement parentContainer;

    public TextureLayerView(UIElement parentContainer) : base(UIElementMode.Static)
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