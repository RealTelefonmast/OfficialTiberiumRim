using UnityEngine;

namespace TeleCore.Types.Interfaces;

public interface IReorderableElement
{
    public Rendering.UI.DynaUI.UIElement Element { get; }
    public void DrawElementInScroller(Rect inRect);
}