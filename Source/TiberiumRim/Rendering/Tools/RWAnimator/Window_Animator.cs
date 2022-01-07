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

        public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);
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
            toolBar = new ToolBar();
            canvas = new TextureCanvas(new Rect(50,50, 800, 800));
            canvas.TimeLine = timeLine;
            timeLine.Canvas = canvas;
            browser = new ObjectBrowser(new Rect(850, 50, 275, 700));

            //window.AddElement(timeLine);
            //window.AddElement(toolBar);
            //window.AddElement(canvas);
            //window.AddElement(browser);

            toolBar.AddElement(new SpriteSheetEditor(), new Vector2(100, 100));
            toolBar.AddElement(canvas);
            toolBar.AddElement(browser);

            //canvas.AddElement(new GraphicElement(DefDatabase<HarvesterKindDef>.GetNamed("Harvester_Nod").lifeStages.First().bodyGraphicData.Graphic), new Vector2(15,15));
            //canvas.AddElement(new GraphicElement(DefDatabase<HarvesterKindDef>.GetNamed("Harvester_GDI").lifeStages.First().bodyGraphicData.Graphic), new Vector2(30, 30));
        }

        public override void PreOpen()
        {
            base.PreOpen();
        }


        public override void DoWindowContents(Rect inRect)
        {
            //TRWidgets.DrawBoxHighlight(inRect);

            //Rect canvasRect = new Rect(InitialSize.x / 2 - CanvasSize.x / 2, InitialSize.y / 2 - CanvasSize.y / 2, CanvasSize.x, CanvasSize.y);
            Rect toolBarRect = inRect.RightPartPixels(125).TopHalf();

            canvas.DrawElement();
            browser.DrawElement();
            toolBar.DrawElement(toolBarRect);

            Rect timeLineRect = new Rect(inRect.BottomPart(0.15f));
            timeLine.DrawElement(timeLineRect);


            UIDragNDropper.DrawCurDrag();
        }
    }
}
