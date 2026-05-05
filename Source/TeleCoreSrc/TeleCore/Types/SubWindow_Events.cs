// TODO: Decouple from TiberiumRim-specific types (TRUtils, TRLog, TRContent, TRCDefOf, TRThingDef)

using TeleCore.Types.Exposables;
using UnityEngine;
using Verse;

namespace TeleCore.Types;

public class SubWindow_Events
{
    private static readonly Vector2 eventGroupSize = new(220, 30);
    private Vector2 eventScrollPos = Vector2.zero;

    public void DrawMenu(Rect rect)
    {
        Widgets.BeginGroup(rect);
        var outRect = new Rect(0, 0, rect.width, rect.height);
        var viewRect = new Rect(0, 0, outRect.width, outRect.height);
        Widgets.BeginScrollView(outRect, ref eventScrollPos, viewRect);
        float curY = 0; //new Vector2(rect.width, 0); //Width and yPos
        foreach (var TRevent in TRUtils.EventManager().allEvents)
        {
            DrawEvent(TRevent, new Rect(0, curY, eventGroupSize.x, eventGroupSize.y));
            curY += eventGroupSize.y;
        }

        Widgets.EndScrollView();
        Widgets.EndGroup();
    }

    public void DrawMain(Rect rect)
    {
    }

    private static void DrawEvent(BaseEvent baseEvent, Rect rect)
    {
        //BaseEvent activeEvent = TRUtils.EventManager().activeEvents.First(e => e != null && e.props == props);
        Widgets.DrawMenuSection(rect);
        Widgets.Label(rect, baseEvent.def.LabelCap + " " + baseEvent.TimeReadOut + " " + baseEvent.def.IsFinished);
    }
}