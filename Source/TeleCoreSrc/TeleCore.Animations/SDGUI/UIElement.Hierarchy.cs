using System.Collections.Generic;

namespace TeleCore.SDGUI;

public partial class UIElement
{
    public UIElement Parent => DGUI_References.ParentOf(this);
    public IReadOnlyCollection<UIElement> Children => DGUI_References.ChildrenOf(this);

    public void AddChild(UIElement element)
    {
        DGUI_References.Reference(this, element);
    }

    private void Hierarchy_Notify_Repaint()
    {
        foreach (var element in Children)
        {
            element.Repaint();
        }
    }
}