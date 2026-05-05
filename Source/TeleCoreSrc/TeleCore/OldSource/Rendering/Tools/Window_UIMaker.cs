using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Tools;

public class Window_UIMaker : Window
{
    public override Vector2 InitialSize => new(Verse.UI.screenWidth, Verse.UI.screenHeight);
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