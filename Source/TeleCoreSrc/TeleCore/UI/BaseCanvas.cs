using System.Collections.Generic;
using TeleCore.Rendering.UI.DynaUI;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;
using IDragAndDropReceiver = TeleCore.Unsorted.IDragAndDropReceiver;
using UIDragNDropper = TeleCore.Unsorted.UIDragNDropper;
using UIElement = TeleCore.Rendering.UI.DynaUI.UIElement;

namespace TeleCore.UI;

public class BaseCanvas : UIElement, IDragAndDropReceiver
{
    //
    internal const int _TileSize = 100;
    internal const int _CanvasDimensions = 5;
    internal static FloatRange SizeRange = new(0.01f, _CanvasDimensions);
    internal static Vector2 TileVector = new(_TileSize, _TileSize);
    private static readonly FloatRange ScaleRange = new(0.5f, 20);

    //
    private Vector2? oldDragPos;


    public BaseCanvas(UIElementMode mode) : base(mode)
    {
        //layerView = new TextureLayerView(this);
        //animationMetaData = new AnimationMetaData(this);
        UIDragNDropper.RegisterAcceptor(this);
    }

    public BaseCanvas(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
    {
        UIDragNDropper.RegisterAcceptor(this);
    }

    //
    public Vector2 DragPos { get; protected set; } = Vector2.zero;
    public float CanvasZoomScale { get; protected set; } = 1;

    public Vector2 MouseOnCanvas => (Event.current.mousePosition - TrueOrigin) / CanvasZoomScale;


    //
    public Vector2 Origin => InRect.AtZero().center + DragPos;
    public Vector2 TrueOrigin => InRect.center + DragPos;
    protected Vector2 LimitSize => Size * 1f * CanvasZoomScale;

    //Drag & Drop
    public virtual void DrawHoveredData(object draggedObject, Vector2 pos)
    {
    }

    public virtual bool TryAcceptDrop(object draggedObject, Vector2 pos)
    {
        return false;
    }

    public virtual bool CanAcceptDrop(object draggedObject)
    {
        return false;
    }

    //Basic Property Setters
    public virtual void Reset()
    {
        DragPos = Vector2.zero;
        CanvasZoomScale = 1f;
    }

    //EVENTS
    protected virtual void HandleEvent_OnCanvas(Event ev, bool inContext = false)
    {
    }

    protected sealed override void HandleEvent_Custom(Event ev, bool inContext = false)
    {
        if (Mouse.IsOver(InRect))
            if (CanManipulateAt(ev.mousePosition, InRect))
            {
                if (IsFocused && ev.button == 0)
                {
                    if (ev.type == EventType.MouseDown)
                        oldDragPos = DragPos;

                    //Left Click Pan
                    if (ev.type == EventType.MouseDrag && oldDragPos.HasValue)
                    {
                        var dragDiff = CurrentDragDiff;
                        var oldDrag = oldDragPos.Value;
                        DragPos = new Vector2(oldDrag.x + dragDiff.x, oldDrag.y + dragDiff.y);
                    }
                }

                //MouseWheel Zoom
                if (ev.type == EventType.ScrollWheel)
                {
                    var zoomDelta = ev.delta.y / _TileSize * CanvasZoomScale;
                    CanvasZoomScale = Mathf.Clamp(CanvasZoomScale - zoomDelta, ScaleRange.min, ScaleRange.max);
                    if (CanvasZoomScale < ScaleRange.max && CanvasZoomScale > ScaleRange.min)
                        DragPos += MouseOnCanvas * zoomDelta;
                }

                //Clear data always
                if (ev.type == EventType.MouseUp) oldDragPos = null;
            }

        //
        HandleEvent_OnCanvas(ev, inContext);
    }

    protected virtual bool CanManipulateAt(Vector2 mousePos, Rect inRect)
    {
        return true;
    }

    //DRAWING
    private void DrawCanvasGrid(Rect inRect)
    {
        Verse.Widgets.BeginGroup(inRect);
        {
            //Limit rect
            var dimension = 5;
            var tileSize = _TileSize * CanvasZoomScale;
            var limitSize = new Vector2(tileSize, tileSize) * dimension;
            var canvasRect = Origin.RectOnPos(limitSize).Rounded();
            TWidgets.DrawColoredBox(canvasRect, TColor.BGDarker, TColor.White05, 1);

            GUI.color = TColor.White025;
            var curX = canvasRect.x;
            var curY = canvasRect.y;
            for (var x = 0; x < dimension; x++)
            {
                Verse.Widgets.DrawLineVertical(curX, canvasRect.y, canvasRect.height);
                Verse.Widgets.DrawLineHorizontal(canvasRect.x, curY, canvasRect.width);
                curY += tileSize;
                curX += tileSize;
            }

            GUI.color = TColor.White05;
            Verse.Widgets.DrawLineHorizontal(Origin.x - limitSize.x / 2, Origin.y, limitSize.x);
            Verse.Widgets.DrawLineVertical(Origin.x, Origin.y - limitSize.y / 2, limitSize.y);
            GUI.color = Color.white;
        }
        Verse.Widgets.EndGroup();
    }

    //Drawing
    protected virtual void DrawOnCanvas(Rect inRect)
    {
    }

    protected sealed override void DrawContentsBeforeRelations(Rect inRect)
    {
        DrawCanvasGrid(inRect);
    }

    protected sealed override void DrawContentsAfterRelations(Rect inRect)
    {
        DrawOnCanvas(inRect);
    }

    //
    protected override IEnumerable<FloatMenuOption> RightClickOptions()
    {
        yield return new FloatMenuOption("Recenter...", delegate { DragPos = Vector2.zero; });
    }
}