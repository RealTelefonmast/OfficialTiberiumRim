using System;
using System.Collections.Generic;
using TeleCore.Static;
using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Tools;

public class DevToolDef : Def
{
    public Type windowClass;
    private Window windowInt;

    public Window GetWindow => windowInt ??= (Window) Activator.CreateInstance(windowClass);
}

public class Dialog_ToolSelection : Window
{
    private List<DevToolDef> allDevTools;

    public Dialog_ToolSelection()
    {
        forcePause = true;
        doCloseX = true;
        doCloseButton = true;
        closeOnClickedOutside = true;
        absorbInputAroundWindow = true;
    }

    public override Vector2 InitialSize => new(900f, 700f);

    public override void PreOpen()
    {
        base.PreOpen();
        allDevTools ??= DefDatabase<DevToolDef>.AllDefsListForReading;
    }

    public override void DoWindowContents(Rect inRect)
    {
        var titleRect = inRect.TopPart(0.05f);
        var selectionRect = inRect.BottomPart(.95f);
        Text.Font = GameFont.Medium;
        Verse.Widgets.Label(titleRect, StringCache.ToolSelection);
        Text.Font = default;

        Verse.Widgets.BeginGroup(selectionRect);
        var list = new List<ListableOption>();
        foreach (var devTool in allDevTools)
            list.Add(new ListableOption(devTool.LabelCap, () => { Find.WindowStack.Add(devTool.GetWindow); }));
        OptionListingUtility.DrawOptionListing(new Rect(0, 0, 200, selectionRect.height), list);
        Verse.Widgets.EndGroup();
    }
}