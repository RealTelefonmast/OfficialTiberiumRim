using UnityEngine;

namespace TeleCore.Unsorted;

public interface IReorderableElement
{
    public UIElement Element { get; }
    public void DrawElementInScroller(Rect inRect);
}