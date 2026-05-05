using UnityEngine;

namespace TeleCore.Unsorted;

public interface IReorderableElement
{
    public Rendering.UI.DynaUI.UIElement Element { get; }
    public void DrawElementInScroller(Rect inRect);
}