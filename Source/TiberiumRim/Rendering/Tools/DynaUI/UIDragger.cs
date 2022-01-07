using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection.Configuration;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class UIDragger
    {
        private static IDraggable curDragged;
        //Position metadata is nullable so we know the positions *at the start* of dragging and save it then
        private static Vector2? startDraggingPos, oldPosition;

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

            if (element.Equals(curDragged))
            {
                startDraggingPos ??= ev.mousePosition;
                oldPosition ??= curDragged.Position;
                Vector2 diff = ev.mousePosition - startDraggingPos.Value;
                curDragged.SetPosition(new Vector2(oldPosition.Value.x + diff.x, oldPosition.Value.y + diff.y));
            }

            if (ev.type == EventType.MouseUp || !IsInsideOfContext(curDragged))
            {
                StopDragging(element);
            }
        }

        private static void StartDragging(IDraggable element, Event ev)
        {
            if (curDragged != null)
            {
                if (curDragged.Priority < element.Priority) return;
            }
            curDragged = element;
        }

        private static void StopDragging(IDraggable element)
        {
            if(element.Equals(curDragged)) 
                curDragged = null;
            oldPosition = startDraggingPos = null;
        }
    }
}
