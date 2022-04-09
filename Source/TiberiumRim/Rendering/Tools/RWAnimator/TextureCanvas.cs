using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TextureCanvas : UIContainer, IDragAndDropReceiver
    {
        //Layer Scroller
        private TextureLayerView layerView;

        //Internal Data
        public TextureElement ActiveTexture => layerView.ActiveElement;
        public TimeLineControl TimeLine { get; set; }

        public override string Label => "Canvas";

        public override UIContainerMode ContainerMode => UIContainerMode.Reverse;

        //Render Data
        private Vector2? oldDragPos;
        private float zoomScale = 1;
        private FloatRange scaleRange = new FloatRange(0.25f, 4);

        public Vector2 Origin => InRect.AtZero().center + DragPos;
        public Vector2 TrueOrigin => InRect.center + DragPos;
        public Vector2 DragPos { get; private set; } = Vector2.zero;
        public float CanvasZoomScale => zoomScale;

        //Event stuff
        public Vector2 MousePos => (Event.current.mousePosition - TrueOrigin) / CanvasZoomScale;

        public TextureCanvas(Rect rect) : base(rect)
        {
            layerView = new TextureLayerView(this);
            UIDragNDropper.RegisterAcceptor(this);
        }

        protected override void Notify_AddedElement(UIElement newElement)
        {
            layerView.Notify_NewLayer(newElement as TextureElement);
            TimeLine.Notify_NewElement(newElement as TextureElement);
        }

        protected override void Notify_RemovedElement(UIElement newElement)
        {
            layerView.Notify_RemovedLayer(newElement);
            TimeLine.Notify_RemovedElement(newElement);
        }

        protected override void Notify_ChildElementChanged(UIElement element)
        {
            base.Notify_ChildElementChanged(element);
        }

        protected override void HandleEvent_Custom(Event ev, bool inContext = false)
        {
            base.HandleEvent_Custom(ev, inContext);
            if (Mouse.IsOver(InRect))
            {
                if (IsFocused /*&& (layerView.ActiveElement == null || layerView.ActiveElement.ManiMode == ManipulationMode.None)*/)
                {
                    if (ev.type == EventType.MouseDown)
                    {
                        oldDragPos = DragPos;
                    }

                    if (ev.type == EventType.MouseDrag)
                    {
                        var dragDiff = (CurrentDragDiff);
                        var oldDrag = oldDragPos.Value;
                        DragPos = new Vector2(oldDrag.x + dragDiff.x, oldDrag.y + dragDiff.y);
                    }

                    if (ev.type == EventType.MouseUp)
                    {
                        oldDragPos = null;
                    }
                }

                //
                if (ev.type == EventType.ScrollWheel)
                {
                    var zoomDelta = (ev.delta.y / 100f);
                    zoomScale = Mathf.Clamp(zoomScale - zoomDelta, scaleRange.min, scaleRange.max);
                    if(zoomScale < scaleRange.max && zoomScale > scaleRange.min)
                        DragPos += MousePos * zoomDelta;
                }
            }
        }

        protected override IEnumerable<FloatMenuOption> RightClickOptions()
        {
            yield return new FloatMenuOption("Recenter...", delegate
            {
                DragPos = Vector2.zero;
            });
        }

        protected override void DrawContents(Rect inRect)
        {
            DrawGrid(inRect);
            base.DrawContents(inRect);
            TRWidgets.DoTinyLabel(inRect, $"Focused: {UIEventHandler.FocusedElement}\n{Event.current.mousePosition}\n{MousePos}");

            CanvasCursor.Notify_TriggeredMode(ActiveTexture?.LockedInMode);

            //
            Widgets.BeginGroup(inRect);
            ActiveTexture?.DrawSelOverlay();
            Widgets.EndGroup();
        }

        private void DrawGrid(Rect inRect)
        {
            Widgets.BeginGroup(inRect);
            DrawCanvasGuidelines();
            Widgets.EndGroup();
            /*
            var size = inRect.size * CanvasZoomScale;
            Widgets.BeginGroup(inRect);
            TRWidgets.DrawGridOnCenter(new Rect(TrueOrigin.x - size.x / 2, TrueOrigin.y - size.y / 2, size.x, size.y), 100 * CanvasZoomScale, TrueOrigin);
            Widgets.EndGroup();
            */
            //TRWidgets.DrawGrid(inRect, 100f, CanvasScale, LastZoomPos);

            //LayerView
            Rect viewRect = new Rect((Position.x + Size.x) - 1, Position.y, 150, Size.y);
            layerView.DrawElement(viewRect);
        }

        private Vector2 LimitSize => (Size*1f) * CanvasZoomScale;

        private void DrawCanvasGuidelines()
        {
            //Limit rect
            //TRWidgets.DrawBox(Origin.RectOnPos(LimitSize), TRMats.White075, 2);
            TRWidgets.DrawColoredBox(Origin.RectOnPos(LimitSize), TRMats.BGDarker, TRColor.White05, 1);
            GUI.color = TRColor.White025;
            Widgets.DrawLineHorizontal(Origin.x-LimitSize.x/2, Origin.y, LimitSize.x);
            Widgets.DrawLineVertical(Origin.x, Origin.y - LimitSize.y / 2, LimitSize.y);
            GUI.color = Color.white;
        }

        private void DrawSelectedProperties(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            Widgets.Label(rect, $"Properties");

            //Widgets.TextFieldNumericLabeled();
        }

        //Dragging
        public void DrawHoveredData(object draggedData, Vector2 pos)
        {
            GUI.color = TRColor.White05;
            if (draggedData is WrappedTexture tex)
            {
                var texture = tex.texture;
                Rect drawRect = pos.RectOnPos((new Vector2(texture.width, texture.height) / 2) * CanvasZoomScale);
                Widgets.DrawTextureFitted(drawRect, texture, 1);
                TRWidgets.DoTinyLabel(drawRect, $"{pos}");
                TRWidgets.DrawBox(drawRect, Color.black, 1);
            }

            if (draggedData is SpriteTile tile)
            {
                Rect drawRect = pos.RectOnPos((tile.rect.size / 2) * CanvasZoomScale);
                tile.DrawTile(drawRect);
                TRWidgets.DoTinyLabel(drawRect, $"{pos}");
                TRWidgets.DrawBox(drawRect, Color.black, 1);
            }
            GUI.color = Color.white;
        }

        public bool TryAccept(object draggedObject, Vector2 pos)
        {
            TextureElement element = null;
            if (draggedObject is WrappedTexture texture)
            {
                element = new TextureElement(new Rect(Vector2.zero, Size), texture);
                element.SetData(parent: this);
                element.SetTRSP_FromScreenSpace(pos);
            }

            if (draggedObject is SpriteTile tile)
            {
                element = new TextureElement(new Rect(Vector2.zero, Size), tile.spriteMat, tile.normalRect);
                element.SetData(parent: this);
                element.SetTRSP_FromScreenSpace(pos, size:tile.rect.size / 2f, pivot:tile.pivot);
            }

            if (element != null)
            {
                AddElement(element);
                return true;
            }
            return false;
        }

        public bool Accepts(object draggedObject)
        {
            if (draggedObject is WrappedTexture or SpriteTile) return true;
            return false;
        }
    }
}
