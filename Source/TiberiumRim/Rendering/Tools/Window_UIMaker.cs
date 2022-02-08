using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Window_UIMaker : Window
    {
        public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);
        public override float Margin => 5f;

        public override void DoWindowContents(Rect inRect)
        {
            forcePause = true;
            doCloseX = true;
            doCloseButton = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            closeOnCancel = false;

            layer = WindowLayer.Super;
        }
    }
}
