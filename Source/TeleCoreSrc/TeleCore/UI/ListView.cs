using System.Collections.Generic;

namespace TeleCore.Unsorted;

public class ListView<T> : Rendering.UI.DynaUI.UIElement
{
    private List<T> _items;

    protected virtual void DrawListing(T item)
    {
    }

    public override void Draw()
    {
        base.Draw();
    }
}