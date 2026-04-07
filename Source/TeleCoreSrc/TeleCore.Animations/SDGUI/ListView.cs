using UnityEngine;

namespace TeleCore.SDGUI;

public class ListView : UIElement
{
    
    
    public ListView(Rect rect) : base(rect)
    {
    }

    public ListView(Vector2 pos, Vector2 size) : base(pos, size)
    {
    }

    public override void Repaint()
    {
        base.Repaint();
    }

    private void DrawItem()
    {
        
    }
}