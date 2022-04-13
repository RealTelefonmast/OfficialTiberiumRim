using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public enum UIContainerMode
    {
        InOrder,
        Reverse,
    }

    public class UIContainer : UIElement
    {
        protected List<UIElement> elements;

        public List<UIElement> ElementList => elements;

        public virtual UIContainerMode ContainerMode => UIContainerMode.InOrder;

        public UIContainer(UIElementMode mode) : base(mode)
        {
            elements = new List<UIElement>();
        }

        public UIContainer(Rect rect, UIElementMode mode) : base(rect, mode)
        {
            elements = new List<UIElement>();
        }

        public UIContainer(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
        {
            elements = new List<UIElement>();
        }

        //Data Notifiers
        protected virtual void Notify_AddedElement(UIElement newElement)
        {

        }

        protected virtual void Notify_RemovedElement(UIElement newElement)
        {

        }

        protected override void Notify_ChildElementChanged(UIElement element)
        {
            base.Notify_ChildElementChanged(element);
        }

        //
        public void AddElement(UIElement newElement)
        {
            newElement.SetData(parent: this);
            switch (ContainerMode)
            {
                case UIContainerMode.InOrder:
                    elements.Add(newElement);
                    break;
                case UIContainerMode.Reverse:
                    elements.Insert(0, newElement);
                    break;
            }
            Notify_AddedElement(newElement);
        }

        public void AddElement(UIElement newElement, Vector2 pos)
        {
            newElement.SetData(pos, parent: this);
            elements.Add(newElement);
            Notify_AddedElement(newElement);
        }

        public void Discard(UIElement element)
        {
            elements.Remove(element);
            Notify_RemovedElement(element);
        }

        protected override void DrawContents(Rect inRect)
        {
            Widgets.BeginGroup(inRect);
            switch (ContainerMode)
            {
                case UIContainerMode.InOrder:
                {
                    foreach (var element in elements)
                    {
                        element.DrawElement();
                    }

                    break;
                }
                case UIContainerMode.Reverse:
                {
                    for (int i = elements.Count - 1; i >= 0; i--)
                    {
                        var element = elements[i];
                        element.DrawElement();
                    }

                    break;
                }
            }

            Widgets.EndGroup();
        }
    }
}
