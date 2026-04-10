using UnityEngine;

namespace TeleCore.UI.DynaUI;

public interface IReorderableElement
{
    public UIElement Element { get; }
    public void DrawElementInScroller(Rect inRect);
}