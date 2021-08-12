using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Dialog_SubSetting : Window
    {
        public Dialog_SubSetting()
        {
            base.closeOnClickedOutside = true;
        }

        protected override float Margin => 0f;

        public override Vector2 InitialSize => new Vector2(900f, 520f);

        public override void PostClose()
        {
            Find.WindowStack.Add(new Dialog_TiberiumRimSettings());
        }

        public override void DoWindowContents(Rect inRect)
        {
            Widgets.DrawTextureFitted(inRect, TiberiumContent.MenuWindow, 1f);
        }
    }
}
