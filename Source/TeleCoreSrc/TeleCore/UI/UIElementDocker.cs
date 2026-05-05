using UnityEngine;
using Verse;

namespace TeleCore.UI;

//TODO: Assign docks and manage size/pos of docked elements during docking
public class UIElementDocker : Rendering.UI.DynaUI.UIElement
{
    private readonly Rendering.UI.DynaUI.UIElement[] _dockedElements = new Rendering.UI.DynaUI.UIElement[4];

    public UIElementDocker(UIElementMode mode) : base(mode)
    {
    }

    public UIElementDocker(Rect rect, UIElementMode mode) : base(rect, mode)
    {
    }

    public UIElementDocker(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
    {
    }

    public void DockElement(Rendering.UI.DynaUI.UIElement element, Rot4 dockPos)
    {
        _dockedElements[dockPos.AsInt] = element;
    }
}