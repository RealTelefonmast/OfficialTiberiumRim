using System.Collections.Generic;

namespace TeleCore.Unsorted;

public class UIContainer : Rendering.UI.DynaUI.UIElement
{
    private List<Rendering.UI.DynaUI.UIElement> _elements;

    public sealed override void Draw()
    {
        foreach (var element in _elements) element.DrawInternal();
    }
}