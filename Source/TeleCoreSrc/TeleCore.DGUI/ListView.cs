using System.Collections.Generic;

namespace TeleCore.DGUI;

public class ListView<T> : UIElement
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