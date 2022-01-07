using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TextureLayerView : UIElement, IDragAndDropReceiver
    {
        private UIContainer parentContainer;
        private ElementScroller internalScroller;

        public override UIElementMode UIMode => UIElementMode.Static;

        public TextureElement ActiveElement => internalScroller.SelectedElement as TextureElement;

        public TextureLayerView(UIContainer parentContainer)
        {
            this.parentContainer = parentContainer;
            internalScroller = new ElementScroller(parentContainer);
        }

        public void Notify_NewLayer(TextureElement newElement)
        {
            internalScroller.Notify_NewElement(newElement);
        }

        public void Notify_RemovedLayer(UIElement element)
        {
            internalScroller.Notify_RemoveElement(element);

        }

        protected override void HandleEvent_Custom(Event ev, bool inContext)
        {
            base.HandleEvent_Custom(ev);
        }

        protected override void DrawContents(Rect inRect)
        {
            internalScroller.DrawElement(inRect);
            
            /*
            base.DrawContents(inRect);
            GUI.BeginGroup(inRect);
            inRect = inRect.AtZero();
            Rect scrollRect = new Rect(0, 0, inRect.width, elements.Count * inRect.width);
            Widgets.BeginScrollView(inRect, ref scrollVec, scrollRect, false);

            float curY = 0;
            for (var i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                var sFlag = i % 2 != 0;
                var sameFlag = element.Equals(ActiveElement);
                var rect = new Rect(0, curY, inRect.width, inRect.width);
                var mouseOver = Mouse.IsOver(rect);

                if (sFlag)
                {
                    Widgets.DrawBoxSolid(rect, TRMats.White005);
                }

                if (sameFlag || mouseOver)
                {
                    Widgets.DrawHighlight(rect);
                    if (sameFlag)
                    {
                        TRWidgets.DrawBox(rect, Color.cyan, 1);
                    }

                    if (mouseOver)
                    {
                        DragAndDropData = element;
                    }
                }

                if (Widgets.ButtonInvisible(rect))
                {
                    ActiveElement = element;
                }

                var mat = element.Material;
                TRWidgets.DoTinyLabel(rect,
                    $"{mat.mainTexture.name}\n{mat.shader.name}\n{RectSimple(element.texCoords ?? default)}\n{element.pivotPoint}\n{mat.mainTextureOffset}\n{mat.mainTextureScale}");
                curY += inRect.width;
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
            */
        }

        public void DrawHoveredData(object draggedObject, Vector2 pos)
        {

        }

        public bool TryAccept(object draggedObject, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public bool Accepts(object draggedObject)
        {
            throw new NotImplementedException();
        }
    }
}
