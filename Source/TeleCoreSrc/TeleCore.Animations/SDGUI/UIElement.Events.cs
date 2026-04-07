namespace TeleCore.SDGUI;

public partial class UIElement
{
    private void LayoutInt()
    {
        Layout();
    }

    private void RepaintInt()
    {
        Repaint();
        Hierarchy_Notify_Repaint();
    }

    private void MouseDownInt()
    {
        
    }

    private void MouseUpInt()
    {
        
    }

    public virtual void Layout()
    {
        
    }

    public virtual void Repaint()
    {
        
    }
}