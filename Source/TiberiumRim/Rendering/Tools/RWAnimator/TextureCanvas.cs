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

        private Rect DataReadoutRect => new Rect(InRect.xMax - 250, InRect.y + 1, 250, 250);

        //Render Data
        private Vector2? oldDragPos;
        private float zoomScale = 1;
        private FloatRange scaleRange = new FloatRange(0.25f, 4);

        public Vector2 Origin => InRect.AtZero().center + DragPos;
        public Vector2 TrueOrigin => InRect.center + DragPos;
        public Vector2 DragPos { get; private set; } = Vector2.zero;
        private Vector2 LimitSize => (Size * 1f) * CanvasZoomScale;

        public float CanvasZoomScale => zoomScale;

        //Event stuff
        public Vector2 MousePos => (Event.current.mousePosition - TrueOrigin) / CanvasZoomScale;

        public TextureCanvas(UIElementMode mode) : base(mode)
        {
            layerView = new TextureLayerView(this);
            UIDragNDropper.RegisterAcceptor(this);
        }

        protected override void Notify_AddedElement(UIElement newElement)
        {
            base.Notify_AddedElement(newElement);
            layerView.Notify_NewLayer(newElement as TextureElement);
            TimeLine.Notify_NewElement(newElement as TextureElement);
        }

        protected override void Notify_RemovedElement(UIElement newElement)
        {
            base.Notify_RemovedElement(newElement);
            layerView.Notify_RemovedLayer(newElement);
            TimeLine.Notify_RemovedElement(newElement);
        }

        protected override void Notify_ChildElementChanged(UIElement element)
        {
            base.Notify_ChildElementChanged(element);
        }

        protected override void HandleEvent_Custom(Event ev, bool inContext = false)
        {
            if (Mouse.IsOver(InRect))
            {
                if (layerView.DrawDataReadout && Mouse.IsOver(DataReadoutRect))
                {
                    if(ev.type == EventType.MouseDown)
                        UIEventHandler.StartFocus(this, DataReadoutRect);
                    return;
                }
                if (IsFocused && ev.button == 0)
                {
                    if (ev.type == EventType.MouseDown)
                    {
                        oldDragPos = DragPos;
                    }

                    if (ev.type == EventType.MouseDrag && oldDragPos.HasValue)
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
            var element = UIEventHandler.FocusedElement as UIElement;
            var texElement = element as TextureElement;
            TRWidgets.DoTinyLabel(inRect, $"Focused: {element}[{(element)?.RenderLayer}]\n{Event.current.mousePosition}\n{MousePos}\n{(element)?.CurrentDragDiff}" +
                                          $"\n{((element?.StartDragPos - (texElement?.TextureRect.center)) ?? Vector2.zero).normalized}");

            CanvasCursor.Notify_TriggeredMode(ActiveTexture?.LockedInMode);

            //
            Widgets.BeginGroup(inRect);
            ActiveTexture?.DrawSelOverlay();
            Widgets.EndGroup();

            if (layerView.DrawDataReadout)
            {
                DrawReadout(DataReadoutRect, ActiveTexture);
            }
        }

        private void DrawGrid(Rect inRect)
        {
            Widgets.BeginGroup(inRect);
            DrawCanvasGuidelines();
            Widgets.EndGroup();

            //LayerView
            Rect viewRect = new Rect((Position.x + Size.x) - 1, Position.y, 150, Size.y);
            layerView.DrawElement(viewRect);
        }

        private void DrawReadout(Rect rect, TextureElement tex)
        {
            //Transform
            Widgets.DrawMenuSection(rect);
            var ev = Event.current;
            //var mousePos = ev.mousePosition;
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect.ContractedBy(4));
            listing.Label("Transform");
            listing.GapLine();
            listing.Label("Size:");
            var xSize = tex.TSize.x;
            var ySize = tex.TSize.y;
            var xPos = tex.TPosition.x;
            var yPos = tex.TPosition.y;
            var rot = tex.TRotation;

            bool flag = ev.type == EventType.KeyDown;

            listing.DoBGForNext(TRColor.White025);
            listing.TextFieldNumericLabeled("X", ref xSize, ref tex.ValueBuffer[0], float.MinValue,
                anchor: TextAnchor.MiddleLeft);
            listing.DoBGForNext(TRColor.White025);
            listing.TextFieldNumericLabeled("Y", ref ySize, ref tex.ValueBuffer[1], float.MinValue,
                anchor: TextAnchor.MiddleLeft);

            listing.Label("Position:");
            listing.DoBGForNext(TRColor.White025);
            listing.TextFieldNumericLabeled("X", ref xPos, ref tex.ValueBuffer[2], float.MinValue,
                anchor: TextAnchor.MiddleLeft);
            listing.DoBGForNext(TRColor.White025);
            listing.TextFieldNumericLabeled("Y", ref yPos, ref tex.ValueBuffer[3], float.MinValue,
                anchor: TextAnchor.MiddleLeft);

            listing.Label("Rotation:");
            listing.DoBGForNext(TRColor.White025);
            listing.TextFieldNumericLabeled("Rot", ref rot, ref tex.ValueBuffer[4], -360, 360, TextAnchor.MiddleLeft);

            if (flag)
            {
                var newSize = new Vector2(xSize, ySize);
                var newPos = new Vector2(xPos, yPos);
                if (newSize != tex.TSize)
                    tex.TSize = newSize;
                if (newPos != tex.TPosition)
                    tex.TPosition = newPos;
                if (rot != tex.TRotation)
                    tex.TRotation = rot;
            }

            if (OriginalEventUtility.EventType == EventType.MouseDown && !rect.Contains(UIEventHandler.MouseOnScreen))
                GUI.FocusControl(null);

            listing.End();

            //if(!ActiveTexture.SubParts.NullOrEmpty())
                //DrawChildProperties(new Rect(rect.x, rect.yMax, rect.width, rect.height));
        }

        private void DrawChildProperties(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect.ContractedBy(4));
            listing.Label("Sub Parts");
            listing.GapLine();

            foreach (var part in ActiveTexture.SubParts)
            {
                listing.TextureElement(part);
            }

            listing.End();
        }

        private void DrawCanvasGuidelines()
        {
            //Limit rect
            var dimension = 5;
            var tileSize = 100 * CanvasZoomScale;
            var limitSize = (new Vector2(tileSize, tileSize) * dimension);
            var canvasRect = Origin.RectOnPos(limitSize).Rounded();
            TRWidgets.DrawColoredBox(canvasRect, TRMats.BGDarker, TRColor.White05, 1);

            GUI.color = TRColor.White025;
            var curX = canvasRect.x;
            var curY = canvasRect.y;
            for (int x = 0; x < dimension; x++)
            {
                Widgets.DrawLineVertical(curX, canvasRect.y, canvasRect.height);
                Widgets.DrawLineHorizontal(canvasRect.x, curY, canvasRect.width);
                curY += tileSize;
                curX += tileSize;
            }

            GUI.color = TRColor.White05;
            Widgets.DrawLineHorizontal(Origin.x - limitSize.x / 2, Origin.y, limitSize.x);
            Widgets.DrawLineVertical(Origin.x, Origin.y - limitSize.y / 2, limitSize.y);
            GUI.color = Color.white;
        }

        //Dragging
        public void DrawHoveredData(object draggedData, Vector2 pos)
        {
            GUI.color = TRColor.White05;
            if (draggedData is WrappedTexture tex)
            {
                var texture = tex.Texture;
                Rect drawRect = pos.RectOnPos(new Vector2(100, 100) * CanvasZoomScale);
                Widgets.DrawTextureFitted(drawRect, texture, 1);
                TRWidgets.DoTinyLabel(drawRect, $"{pos}");
                TRWidgets.DrawBox(drawRect, Color.black, 1);
            }

            if (draggedData is SpriteTile tile)
            {
                Rect drawRect = pos.RectOnPos(((tile.normalRect.size) * new Vector2(100,100)) * CanvasZoomScale);
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
                AddElement(element);
                element.SetTRSP_FromScreenSpace();
            }

            if (draggedObject is SpriteTile tile)
            {
                element = new TextureElement(new Rect(Vector2.zero, Size), tile.spriteMat, tile.normalRect);
                AddElement(element);
                element.SetTRSP_FromScreenSpace(pivot:tile.pivot);
            }

            return element != null;
        }

        public bool Accepts(object draggedObject)
        {
            if (draggedObject is WrappedTexture or SpriteTile) return true;
            return false;
        }
    }
}
