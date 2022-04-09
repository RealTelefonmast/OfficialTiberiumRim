using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ToolBar : UIContainer
    {
        public override UIElementMode UIMode => UIElementMode.Static;

        public ToolBar() : base()
        {

        }

        public ToolBar(Rect rect) : base(rect)
        {
        }

        public ToolBar(Vector2 pos, Vector2 size) : base(pos, size)
        {
        }

        protected override void Notify_AddedElement(UIElement newElement)
        {
            newElement.ToggleOpen();
        }

        protected override void DrawContents(Rect inRect)
        {
            Widgets.BeginGroup(inRect);
            List<ListableOption> list = new List<ListableOption>();
            foreach (UIElement element in elements)
            {
                list.Add(new ListableOption(element.Label, () => { element.ToggleOpen(); }));
            }
            OptionListingUtility.DrawOptionListing(new Rect(0, 0, inRect.width, inRect.height), list);
            Widgets.EndGroup();

            //Draw Each Tool
            foreach (var element in elements)
            {
                element.DrawElement();
            }
        }
    }
}
