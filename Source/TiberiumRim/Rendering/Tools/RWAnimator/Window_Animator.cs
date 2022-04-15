using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    //TODO: Add way to quick test animation ingame via button and then get back to animation tool
    public class Window_Animator : Window
    {
        private UIContainer window;

        private TimeLineControl timeLine;
        private TextureCanvas canvas;
        private ObjectBrowser browser;
        private ToolBar toolBar;
        private AnimationSaveLoader saveLoader;

        public sealed override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);
        private Vector2 CanvasSize => new(800, 800);
        public override float Margin => 5f;

        public Window_Animator()
        {
            forcePause = true;
            doCloseX = true;
            doCloseButton = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            closeOnCancel = false;

            layer = WindowLayer.Super;

            //
            //window = new UIContainer();

            timeLine = new TimeLineControl();
            toolBar = new ToolBar(UIElementMode.Static);
            canvas = new TextureCanvas(UIElementMode.Static);
            canvas.TimeLine = timeLine;
            timeLine.Canvas = canvas;
            browser = new ObjectBrowser(new Rect(850, 50, 350, 700), UIElementMode.Dynamic);
            saveLoader = new AnimationSaveLoader(canvas, new Rect(InitialSize.x - (125 + 650), 0, 650 - 125, 500), UIElementMode.Static);
            
            //toolBar.AddElement(canvas);
            toolBar.AddElement(new SpriteSheetEditor(UIElementMode.Dynamic), new Vector2(100, 100));
            toolBar.AddElement(browser);
            toolBar.AddElement(saveLoader);
        }

        public override void PreOpen()
        {
            base.PreOpen();
        }

        public override void DoWindowContents(Rect inRect)
        {
            UIEventHandler.CurrentLayer = 0;
            Rect topRect = inRect.TopPart(0.85f).Rounded();
            Rect canvasRect = topRect.LeftPartPixels(900);
            Rect toolBarRect = inRect.RightPartPixels(125).TopHalf();
            Rect timeLineRect = inRect.BottomPart(0.15f).Rounded();

            UIEventHandler.Notify_MouseOnScreen(Event.current.mousePosition);

            canvas.DrawElement(canvasRect);
            toolBar.DrawElement(toolBarRect);
            timeLine.DrawElement(timeLineRect);

            UIDragNDropper.DrawCurDrag();

            UIEventHandler.Notify_MouseOnScreen(Vector2.zero);
        }
    }
}
