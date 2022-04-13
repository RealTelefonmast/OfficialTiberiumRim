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
    public class TextureLayerView : UIElement
    {
        private UIContainer parentContainer;
        private ElementScroller internalScroller;

        public TextureElement ActiveElement => internalScroller.SelectedElement as TextureElement;

        public bool DrawDataReadout => ActiveElement != null;

        public TextureLayerView(UIContainer parentContainer) : base(UIElementMode.Static)
        {
            this.parentContainer = parentContainer;
            internalScroller = new ElementScroller(parentContainer, UIElementMode.Static);
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
            Rect rect = new Rect(inRect.x - 1, inRect.y, inRect.width + 2, inRect.height);
            internalScroller.DrawElement(rect);
        }
    }
}
