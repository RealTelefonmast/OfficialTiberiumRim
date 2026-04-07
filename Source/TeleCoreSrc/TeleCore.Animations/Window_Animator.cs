using UnityEngine;
using Verse;

namespace TeleCore;

public class DevTool_Animation : Window
{
    internal AnimationWindowContainer content;

    public DevTool_Animation()
    {
        forcePause = true;
        doCloseX = false;
        doCloseButton = false;
        closeOnClickedOutside = false;
        absorbInputAroundWindow = true;
        closeOnAccept = false;
        closeOnCancel = false;

        layer = WindowLayer.Super;

        //
        content = new AnimationWindowContainer(this, new Rect(Vector2.zero, InitialSize), UIElementMode.Static);
    }

    public sealed override Vector2 InitialSize => new(UI.screenWidth, UI.screenHeight);
    public override float Margin => 5f;

    public override void PreOpen()
    {
        base.PreOpen();
        content.Notify_Reopened();
    }

    public override void DoWindowContents(Rect inRect)
    {
        UIEventHandler.Begin();
        content.DrawElement(inRect);
        UIDragNDropper.DrawCurDrag();
        UIEventHandler.End();
    }
}