using UnityEngine;

namespace TeleCore.DGUI;

public class UIElement
{
    private Vector2 _pos;
    private Vector2 _size;
    private Rect _rect;
    
    internal void DrawInternal()
    {
        Draw();
    }

    public virtual void ProcessEvent()
    {
        
    }
    
    public virtual void Draw()
    {
        
    }
}