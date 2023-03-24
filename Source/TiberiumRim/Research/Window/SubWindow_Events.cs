using UnityEngine;
using Verse;

namespace TiberiumRim.Research.Window;

public class SubWindow_Events
{
    private static Vector2 
    
    public void DrawLeft(Rect rect)
    {
        Widgets.BeginGroup(rect);
        Rect outRect = new Rect(0, 0, rect.width, rect.height - bannerHeight);
        Rect viewRect = new Rect(0, 0, outRect.width, outRect.height);
        Widgets.BeginScrollView(outRect, ref projectScrollPos, viewRect, true);
        float curY = 0; //new Vector2(rect.width, 0); //Width and yPos
        foreach (var TRevent in TRUtils.EventManager().allEvents)
        {
            DrawEvent(TRevent, new Rect(0, curY, researchGroupSize.x, researchGroupSize.y ));
            curY += researchGroupSize.y;
        }
        Widgets.EndScrollView();
        Widgets.EndGroup();
    }

    public void DrawRight()
    {
        
    }

    private static void DrawEvent(BaseEvent baseEvent, Rect rect)
    {
        //BaseEvent activeEvent = TRUtils.EventManager().activeEvents.First(e => e != null && e.props == props);
        Widgets.DrawMenuSection(rect);
        Widgets.Label(rect, baseEvent.def.LabelCap + " " + baseEvent.TimeReadOut + " " + baseEvent.def.IsFinished);
    }
}