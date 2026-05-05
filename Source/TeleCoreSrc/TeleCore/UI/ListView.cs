using System.Collections.Generic;

namespace TeleCore.UI;

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