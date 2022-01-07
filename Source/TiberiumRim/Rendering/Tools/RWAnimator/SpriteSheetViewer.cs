using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TiberiumRim
{
    public class SpriteSheetViewer : UIElement, IDragAndDropReceiver
    {
        public override UIElementMode UIMode => UIElementMode.Static;

        public SpriteSheetViewer() : base()
        {
            hasTopBar = false;
        }

        protected override void HandleEvent_Custom(Event ev, bool inContext = false)
        {
            base.HandleEvent_Custom(ev, inContext);
        }

        protected override void DrawContents(Rect inRect)
        {
            base.DrawContents(inRect);
        }

        //DragNDrop
        public void DrawHoveredData(object draggedObject, Vector2 pos)
        {
            throw new NotImplementedException();
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
