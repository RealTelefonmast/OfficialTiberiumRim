using System.Collections.Generic;

namespace TeleCore.Unsorted;

public class UIContainer : UIElement
{
    private List<UIElement> _elements;

    public sealed override void Draw()
    {
        foreach (var element in _elements)
        {
            element.DrawInternal();
        }
    }
}