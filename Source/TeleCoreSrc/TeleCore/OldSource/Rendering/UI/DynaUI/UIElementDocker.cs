using UnityEngine;
using Verse;

namespace TeleCore.Rendering.UI.DynaUI;

//TODO: Assign docks and manage size/pos of docked elements during docking
public class UIElementDocker : UIElement
{
    private readonly UIElement[] _dockedElements = new UIElement[4];

    public UIElementDocker(UIElementMode mode) : base(mode)
    {
    }

    public UIElementDocker(Rect rect, UIElementMode mode) : base(rect, mode)
    {
    }

    public UIElementDocker(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
    {
    }

    public void DockElement(UIElement element, Rot4 dockPos)
    {
        _dockedElements[dockPos.AsInt] = element;
    }
}