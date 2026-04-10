using UnityEngine;

namespace TeleCore.UI.DynaUI;

public interface IDraggable
{
    public Vector2 Position { get; set; }
    public Rect? DragContext { get; }
    public Rect Rect { get; }
    public int Priority { get; }
}