using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.Types.Utils;

public static class WidgetStackPanel
{
    private const float DistFromMouse = 26f;
    private const float LabelColumnWidth = 130f;
    private const float InfoColumnWidth = 170f;
    private const float WindowPadding = 12f;
    private const float Padding = 6f;
    private const float LineHeight = 24f;
    private const float ThingIconSize = 22f;

    private static bool active;
    private static int numLines;
    private static Vector2 startPos;
    private static float width;
    private static float widthHalf;

    //Dynamic
    private static Vector2 curXY;

    public static Rect Rect => new(curXY.x, curXY.y, width, LineHeight * numLines);

    public static void Begin(Rect rect)
    {
        if (active)
        {
            TLog.Warning("Trying to push new stackpanel while another is active");
            return;
        }

        numLines = 0;
        active = true;
        startPos = curXY = rect.position;
        width = rect.width;
        widthHalf = width * 0.5f;
    }

    public static void End()
    {
        numLines = 0;
        active = false;
    }

    private static void Increment(float y_Val = LineHeight)
    {
        numLines++;
        curXY += new Vector2(0, y_Val);
    }

    //
    public static void DrawHeader(string text)
    {
        Text.Anchor = TextAnchor.UpperCenter;
        Text.Font = GameFont.Small;

        var rectHeight = Text.CalcHeight(text, width);
        var rect = new Rect(curXY.x, curXY.y + Padding, width, rectHeight);
        Widgets.Label(rect, text);

        Text.Font = default;
        Text.Anchor = default;

        //
        Increment(rect.height + Padding);
    }

    public static void DrawWidgetRow(WidgetRow row)
    {
        var num = numLines * 24f;
        var curY = curXY.y + num + 12f;
        var rect = new Rect(curXY.x, curY, width, LineHeight);
        row.Init(curXY.x, curY, UIDirection.RightThenDown, width);
    }

    public static void DrawRow(string label, string info)
    {
        var rect = new Rect(curXY.x, curXY.y + Padding, width, LineHeight);
        var rect1 = rect.LeftPartPixels(widthHalf);
        var rect2 = rect.RightPartPixels(widthHalf);

        Widgets.Label(rect1, label);
        Widgets.Label(rect2, info);

        /*
        float num = numLines * LineHeight;
        var curY = curXY.y + num + Padding;
        Rect rect = new Rect(curXY.x, curY, width, LineHeight);

        if (numLines % 2 == 1)
            Widgets.DrawLightHighlight(rect);

        //Label Part
        GUI.color = Color.gray;
        Rect rect2 = rect.ContractedBy(Padding, 0).LeftPartPixels(LabelColumnWidth); //new Rect(curXY.x + Padding, curY, LabelColumnWidth, LineHeight);
        Widgets.Label(rect2, label);


        //Info Part
        GUI.color = Color.white;
        Rect rect3 = rect.ContractedBy(Padding, 0).RightPartPixels(LabelColumnWidth);  //new Rect(rect2.xMax + Padding, rect2.y, width - (rect2.xMax + Padding), LineHeight);
        Widgets.Label(rect3, info);
        //TooltipHandler.TipRegion(rect, info);
        */

        Increment(rect.height + Padding);
    }

    public static void DrawThingRow(Thing thing)
    {
        var num = numLines * 24f;
        List<object> selectedObjects = Find.Selector.SelectedObjects;
        var rect = new Rect(12f, num + 12f, width, 24f);
        if (selectedObjects.Contains(thing))
            Widgets.DrawHighlight(rect);
        else if (numLines % 2 == 1) Widgets.DrawLightHighlight(rect);

        rect = new Rect(24f, num + 12f + 1f, 22f, 22f);
        if (thing is Blueprint || thing is Frame)
            Widgets.DefIcon(rect, thing.def);
        else if (thing is Pawn || thing is Corpse)
            Widgets.ThingIcon(rect.ExpandedBy(5f), thing);
        else
            Widgets.ThingIcon(rect, thing);

        rect = new Rect(58f, num + 12f, 370f, 24f);
        Widgets.Label(rect, thing.LabelMouseover);

        //
        Increment();
    }

    public static void DrawDivider()
    {
        GUI.color = Color.gray;
        Widgets.DrawLineHorizontal(curXY.x, curXY.y + Padding, width);
        //TWidgets.GapLine(curXY.x, curXY.y, width, 24f);
        GUI.color = Color.white;

        //
        Increment();
    }
}