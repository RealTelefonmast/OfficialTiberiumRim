using UnityEngine;

namespace TeleCore.DynaUI;

public class UIElement
{
    private Vector2 _pos;
    private Vector2 _size;
    private Rect _rect;
    
    public UIElement Content { get; }

    public void Draw()
    {
        
    }

    protected virtual void BeginDraw()
    {
        
    }

    protected virtual void EndDraw()
    {
        
    }
}