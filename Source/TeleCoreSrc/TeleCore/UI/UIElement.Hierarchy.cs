using System.Collections.Generic;

namespace TeleCore.Unsorted;

public partial class UIElement
{
    public Rendering.UI.DynaUI.UIElement Parent => DGUI_References.ParentOf(this);
    public IReadOnlyCollection<Rendering.UI.DynaUI.UIElement> Children => DGUI_References.ChildrenOf(this);

    public void AddChild(Rendering.UI.DynaUI.UIElement element)
    {
        DGUI_References.Reference(this, element);
    }

    private void Hierarchy_Notify_Repaint()
    {
        foreach (var element in Children) element.Repaint();
    }
}