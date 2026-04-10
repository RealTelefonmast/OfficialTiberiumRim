using System.Collections.Generic;
using TeleCore.Rendering.Tools.RWAnimator;
using TeleCore.Static;
using TeleCore.UI.DynaUI;
using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Tools.EffectBuilder;

internal class EffectBuilderWindowContainer : UIElement
{
    private readonly UITopBar topBar;
    private readonly EffectWorkTableView workTable;
    private readonly Window parentWindow;

    //

    public EffectBuilderWindowContainer(Window parent, Rect rect, UIElementMode mode) : base(rect, mode)
    {
        //
        //bgColor = TColor.WindowBGFillColor;
        //borderColor = TColor.WindowBGBorderColor;

        parentWindow = parent;
        bgColor = TColor.BGDarker;
        borderColor = Color.clear;

        //
        workTable = new EffectWorkTableView(Vector2.zero, new Vector2(rect.width - 15, rect.height - 25),
            UIElementMode.Static);

        //
        var buttonMenus = new List<TopBarButtonMenu>();
        //File
        var fileOptions = new List<TopBarButtonOption>();
        fileOptions.Add(new TopBarButtonOption("New", () => { }));
        fileOptions.Add(new TopBarButtonOption("Save/Load", () => { }));

        buttonMenus.Add(new TopBarButtonMenu("File", fileOptions));

        //View
        var viewOptions = new List<TopBarButtonOption>();
        buttonMenus.Add(new TopBarButtonMenu("View", viewOptions));

        topBar = new UITopBar(buttonMenus);
        topBar.AddCloseButton(() => { parentWindow.Close(); });
    }

    public void Notify_Reopened()
    {
    }

    protected override void DrawTopBarExtras(Rect topRect)
    {
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        Verse.Widgets.BeginGroup(inRect);
        {
            //Rect canvasRect = new Rect(0, 0, 900, 900);
            //Rect objectBrowserRect = new Rect(canvasRect.xMax-1, canvasRect.y, 300, canvasRect.height + 1);

            //
            workTable.DrawElement();
        }
        Verse.Widgets.EndGroup();

        //
        topBar.DrawElement(TopRect);
    }
}