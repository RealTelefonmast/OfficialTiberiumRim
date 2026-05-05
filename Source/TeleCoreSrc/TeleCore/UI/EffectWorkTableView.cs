using UnityEngine;
using Verse;

namespace TeleCore.UI;

public class EffectWorkTableView : Rendering.UI.DynaUI.UIElement
{
    private readonly DefBrowser _browser;
    private readonly ElementScroller _elementView;
    private readonly EffectCanvas _workCanvas;


    public EffectWorkTableView(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
    {
        _workCanvas = new EffectCanvas(UIElementMode.Static);
        _elementView = new ElementScroller(_workCanvas, UIElementMode.Static);
        _browser = new DefBrowser(new Vector2(size.x - 225, pos.y), new Vector2(225, size.y), UIElementMode.Static,
            new DefBrowserSettings
            {
                filter = def => def is ThingDef { IsFrame: false, IsBlueprint: false } or FleckDef
            });
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        GUI.BeginGroup(inRect);
        {
            var settingsArea = inRect.TopPartPixels(100);
            var workArea = inRect.BottomPartPixels(inRect.height - settingsArea.height);
            var canvasRect = workArea.LeftPartPixels(workArea.height);
            var scrollView = new Rect(canvasRect.xMax + 1, canvasRect.y, 125, canvasRect.height);

            Widgets.DrawHighlightIfMouseover(settingsArea);
            Widgets.DrawHighlightIfMouseover(workArea);
            Widgets.DrawHighlightIfMouseover(canvasRect);
            Widgets.DrawHighlightIfMouseover(scrollView);

            _workCanvas.DrawElement(canvasRect);
            _elementView.DrawElement(scrollView);
            _browser.DrawElement();
        }
        GUI.EndGroup();
    }
}