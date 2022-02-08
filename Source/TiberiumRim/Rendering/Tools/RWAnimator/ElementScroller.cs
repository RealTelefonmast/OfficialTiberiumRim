using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public interface IReorderableElement
    {
        public UIElement Element { get; }
        public void DrawElementInScroller(Rect inRect);
    }

    public class ElementScroller : UIElement
    {
        private UIContainer parentContainer;

        private List<UIElement> copyList = new();
        //private List<IReorderableElement> elementList = new();
        public List<UIElement> ElementList => parentContainer.ElementList;

        //Internal Dragging
        private Vector2 scrollVec = Vector2.one;
        private UIElement hoveredElement, draggedElement;

        private Vector2 MousePos { get; set; }
        private int CurrentDropIndex => Mathf.FloorToInt((scrollVec.y + MousePos.y) / Rect.width);

        //
        public UIElement SelectedElement { get; private set; }
        public override UIElementMode UIMode => UIElementMode.Static;

        //
        public ElementScroller(UIContainer parentContainer)
        {
            this.parentContainer = parentContainer;
            hasTopBar = false;
        }

        public void Notify_NewElement(UIElement newElement)
        {
            //elementList.Add(newElement);
            SelectedElement = newElement;
        }

        public void Notify_RemoveElement(UIElement element)
        {
            if (SelectedElement == element)
                SelectedElement = ElementList.FirstOrFallback(null);
        }

        protected override void HandleEvent_Custom(Event ev, bool inContext = false)
        {
            if (ev.type == EventType.MouseDown && Mouse.IsOver(Rect))
            {
                draggedElement ??= hoveredElement;
            }

            if (ev.type == EventType.MouseUp)
            {
                //Try drop element
                if (draggedElement != null)
                {
                    MoveInList(ElementList);
                    draggedElement = null;
                }
            }
        }

        private void MoveInList(List<UIElement> list)
        {
            var oldIndex = list.IndexOf(draggedElement);
            var newIndex = CurrentDropIndex;
            if (newIndex <= 0)
            {
                list.Move(oldIndex, 0);
            }
            else if (newIndex >= list.Count)
            {
                list.Move(oldIndex, list.Count - 1);
            }
            else
            {
                list.Move(oldIndex, newIndex);
            }
        }

        protected override void DrawContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            MousePos = Event.current.mousePosition;

            Rect rect2 = inRect.AtZero();
            Rect scrollRect = new Rect(0, 0, rect2.width, ElementList.Count * rect2.width);
            Widgets.BeginScrollView(rect2, ref scrollVec, scrollRect, false);

            bool currentlyDragging = draggedElement != null;
            if (currentlyDragging)
            {
                copyList.AddRange(ElementList);
                MoveInList(copyList);
            }

            var collection = currentlyDragging ? copyList : ElementList;
            float curY = 0;
            for (var i = 0; i < collection.Count; i++)
            {
                var element = collection[i];
                var sFlag = i % 2 != 0;
                var sameFlag = element.Equals(SelectedElement);
                var rect = new Rect(0, curY, rect2.width, rect2.width);
                var mouseOver = Mouse.IsOver(rect);
                curY += rect2.width;

                if (sFlag)
                {
                    Widgets.DrawBoxSolid(rect, TRColor.White005);
                }

                if (sameFlag || mouseOver)
                {
                    if (sameFlag)
                    {
                        Widgets.DrawBoxSolid(rect, TRMats.BlueHighlight);
                        //TRWidgets.DrawBox(rect, Color.cyan, 1);
                    }

                    if (mouseOver)
                    {
                        Widgets.DrawBoxSolid(rect, TRMats.BlueHighlight_Transparent);
                        hoveredElement = element;
                    }
                }

                if(element is IReorderableElement reorder)
                    reorder.DrawElementInScroller(rect);

                if (Widgets.ButtonInvisible(rect))
                {
                    SelectedElement = element;
                }
            }

            Widgets.EndScrollView();
            GUI.EndGroup();

            copyList.Clear();
        }
    }
}
