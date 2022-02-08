using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public interface IDragAndDropReceiver
    {
        public Rect Rect { get; }
        public bool CanAcceptAnything { get; }
        void DrawHoveredData(object draggedObject, Vector2 pos);
        bool TryAccept(object draggedObject, Vector2 pos);
        bool Accepts(object draggedObject);
    }

    public static class UIDragNDropper
    {
        private static UIElement sourceElement;
        private static object draggedObject;

        private static List<IDragAndDropReceiver> knownAcceptors = new();

        private static IEnumerable<IDragAndDropReceiver> ReadyAcceptors => knownAcceptors.Where(a => a.CanAcceptAnything);

        public static void RegisterAcceptor(IDragAndDropReceiver receiver)
        {
            knownAcceptors.Add(receiver);
        }

        public static void Notify_DraggingData(UIElement source, object data, Event ev)
        {
            StartCarrying(source, data);

            if (ev.type == EventType.MouseUp)
            {
                StopCarrying(source, ev);
            }
        }

        private static void StartCarrying(UIElement source, object data)
        {
            if (sourceElement != null) return;
            sourceElement = source;
            draggedObject = data;
        }

        private static void StopCarrying(UIElement element, Event ev)
        {
            foreach (var acceptor in ReadyAcceptors)
            {
                if(element == acceptor) continue;
                if (acceptor.Rect.Contains(ev.mousePosition) && acceptor.TryAccept(draggedObject, ev.mousePosition))
                {
                    break;
                }
            }

            sourceElement = null;
            draggedObject = null;
        }

        public static void DrawCurDrag()
        {
            if (sourceElement == null) return;

            var mousePos = Event.current.mousePosition;
            foreach (var acceptor in ReadyAcceptors)
            {
                if (acceptor.Accepts(draggedObject) && acceptor.Rect.Contains(mousePos))
                {
                    acceptor.DrawHoveredData(draggedObject, mousePos);
                    return;
                }
            }

            //Draw generic hover
            VisualizeDraggedData(draggedObject, mousePos);
        }

        private static void VisualizeDraggedData(object data, Vector2 pos)
        {
            GUI.color = TRColor.White05;
            if (data is Texture texture)
            {
                Rect box = new Rect(pos, new Vector2(45, 45));
                TRWidgets.DrawBoxHighlight(box);
                Widgets.DrawTextureFitted(box.ContractedBy(1), texture, 1f);
                return;
            }

            if (data is SpriteTile droppedTile)
            {
                Rect box = new Rect(pos, new Vector2(45, 45));
                TRWidgets.DrawBoxHighlight(box);
                droppedTile.DrawTile(box);
                return;
            }

            if (data is TextureSpriteSheet sheet)
            {
                SpriteSheetEditor.DrawSpriteSheet(pos, sheet);
                float width = 150;
                float size = 25;
                
                float height = ((float) (Math.Round((sheet.Tiles.Count / 4f), 0, MidpointRounding.AwayFromZero)) * 25);
                width = Math.Min(sheet.Tiles.Count * size, width);
                Rect rect = new Rect(pos, new Vector2(width, height));
                Widgets.DrawBox(rect, 1);

                GUI.BeginGroup(rect);
                Vector2 XY = Vector2.zero;
                foreach (var tile in sheet.Tiles)
                {
                    Rect spriteRect = new Rect(XY, new Vector2(25, 25));
                    
                    tile.DrawTile(spriteRect);

                    if (XY.x + (size * 2) > width)
                    {
                        XY.y += size;
                        XY.x = 0;
                    }
                    else
                    {
                        XY.x += size;
                    }
                }
                GUI.EndGroup();

                return;
            }
            GUI.color = Color.white;
        }

        public static bool IsSource(UIElement source)
        {
            return source.Equals(sourceElement);
        }
    }
}
