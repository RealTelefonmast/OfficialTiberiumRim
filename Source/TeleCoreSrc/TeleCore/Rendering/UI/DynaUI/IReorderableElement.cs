using UnityEngine;

namespace TeleCore.Rendering.UI.DynaUI;

public interface IReorderableElement
{
    public UIElement Element { get; }
    public void DrawElementInScroller(Rect inRect);
}