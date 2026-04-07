using UnityEngine;

namespace TeleCore.Rendering.UI.DynaUI;

public static class UIDragger
{
    private static IDraggable curDragged;

    //Position metadata is nullable so we know the positions *at the start* of dragging and save it then
    private static Vector2? startDragPos, endDragPos, oldPos;

    public static Vector2 StartDragPos => startDragPos ?? Vector2.zero;
    public static Vector2 EndDragPos => endDragPos ?? Vector2.zero;
    public static Vector2 CurrentDragDiff => endDragPos.HasValue ? endDragPos.Value - startDragPos.Value : Vector2.zero;

    public static Vector2 CurrentDragResult => oldPos.HasValue
        ? new Vector2(oldPos.Value.x + CurrentDragDiff.x, oldPos.Value.y + CurrentDragDiff.y)
        : startDragPos.Value;

    public static bool IsBeingDragged(IDraggable element)
    {
        return element.Equals(curDragged);
    }

    //TODO: Fix Context
    public static bool IsInsideOfContext(IDraggable element)
    {
        return true;
        if (!element.DragContext.HasValue) return true;
        return element.DragContext.Value.Contains(element.Position) &&
               element.DragContext.Value.Contains(element.Position + element.Rect.size);
    }

    public static void Notify_ActiveDrag(IDraggable element, Event ev)
    {
        StartDragging(element, ev);

        var mousePos = ev.mousePosition;
        if (element.Equals(curDragged))
        {
            startDragPos ??= mousePos;
            oldPos ??= curDragged.Position;
            endDragPos = mousePos;
            curDragged.Position = CurrentDragResult;
            //curDragged.SetPosition(new Vector2(oldPosition.Value.x + diff.x, oldPosition.Value.y + diff.y));
        }

        if (ev.type == EventType.MouseUp || !IsInsideOfContext(curDragged)) StopDragging(element);
    }

    private static void StartDragging(IDraggable element, Event ev)
    {
        if (curDragged != null)
            if (curDragged.Priority < element.Priority)
                return;
        curDragged = element;
    }

    private static void StopDragging(IDraggable element)
    {
        if (element.Equals(curDragged))
            curDragged = null;

        oldPos = startDragPos = endDragPos = null;
    }
}