using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

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
            Widgets.DrawTextureFitted(inRect, TRMats.MenuBig, 1f);
        }
    }
}
