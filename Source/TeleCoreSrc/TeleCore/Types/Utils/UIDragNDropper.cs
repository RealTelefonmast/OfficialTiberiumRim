using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public interface IDragAndDropReceiver
{
    public Rect Rect { get; }
    public bool CanAcceptAnything { get; }
    void DrawHoveredData(object draggedObject, Vector2 pos);
    bool TryAcceptDrop(object draggedObject, Vector2 pos);
    bool CanAcceptDrop(object draggedObject);
}

public static class UIDragNDropper
{
    private static Rendering.UI.DynaUI.UIElement sourceElement;
    private static object draggedObject;

    private static readonly List<IDragAndDropReceiver> knownAcceptors = new();

    private static IEnumerable<IDragAndDropReceiver> ReadyAcceptors => knownAcceptors.Where(a => a.CanAcceptAnything);

    public static void RegisterAcceptor(IDragAndDropReceiver receiver)
    {
        knownAcceptors.Add(receiver);
    }

    public static void Notify_DraggingData(Rendering.UI.DynaUI.UIElement source, object data, Event ev)
    {
        StartCarrying(source, data);

        if (ev.type == EventType.MouseUp) StopCarrying(source, ev);
    }

    private static void StartCarrying(Rendering.UI.DynaUI.UIElement source, object data)
    {
        if (sourceElement != null) return;
        TLog.Message($"Starting to carry from {source.Label} of {data}");
        sourceElement = source;
        draggedObject = data;
    }

    private static void StopCarrying(Rendering.UI.DynaUI.UIElement element, Event ev)
    {
        foreach (var acceptor in ReadyAcceptors)
        {
            if (element == acceptor) continue;
            if (acceptor.Rect.Contains(ev.mousePosition) &&
                acceptor.TryAcceptDrop(draggedObject, ev.mousePosition)) break;
        }

        sourceElement = null;
        draggedObject = null;
    }

    public static void DrawCurDrag()
    {
        if (sourceElement == null) return;

        var mousePos = Event.current.mousePosition;
        foreach (var acceptor in ReadyAcceptors)
            if (acceptor.CanAcceptDrop(draggedObject) && acceptor.Rect.Contains(mousePos))
            {
                acceptor.DrawHoveredData(draggedObject, mousePos);
                return;
            }

        //Draw generic hover
        VisualizeDraggedData(draggedObject, mousePos);
    }

    private static void VisualizeDraggedData(object data, Vector2 pos)
    {
        GUI.color = TColor.White05;
        if (data is WrappedTexture texture)
        {
            var box = new Rect(pos, new Vector2(45, 45));
            TWidgets.DrawBoxHighlight(box);
            Widgets.DrawTextureFitted(box.ContractedBy(1), texture.Texture, 1f);
            return;
        }

        if (data is SpriteTile droppedTile)
        {
            var box = new Rect(pos, new Vector2(45, 45));
            TWidgets.DrawBoxHighlight(box);
            droppedTile.DrawTile(box);
            return;
        }

        if (data is TextureSpriteSheet sheet)
        {
            SpriteSheetEditor.DrawSpriteSheet(pos, sheet);
            float width = 150;
            float size = 25;

            var height = (float)TMath.Round(sheet.Tiles.Count / 4f, 0, MidpointRounding.AwayFromZero) * 25;
            width = TMath.Min(sheet.Tiles.Count * size, width);
            var rect = new Rect(pos, new Vector2(width, height));
            Widgets.DrawBox(rect);

            Widgets.BeginGroup(rect);
            var XY = Vector2.zero;
            foreach (var tile in sheet.Tiles)
            {
                var spriteRect = new Rect(XY, new Vector2(25, 25));

                tile.DrawTile(spriteRect);

                if (XY.x + size * 2 > width)
                {
                    XY.y += size;
                    XY.x = 0;
                }
                else
                {
                    XY.x += size;
                }
            }

            Widgets.EndGroup();
            return;
        }

        if (data is Def def)
        {
            var fleckMoteTexture = TWidgets.TextureForFleckMote(def);
            var box = new Rect(pos, new Vector2(45, 45));
            TWidgets.DrawBoxHighlight(box);
            Widgets.DrawTextureFitted(box.ContractedBy(1), fleckMoteTexture, 1f);
            return;
        }

        GUI.color = Color.white;
    }

    public static bool IsSource(Rendering.UI.DynaUI.UIElement source)
    {
        return source.Equals(sourceElement);
    }
}