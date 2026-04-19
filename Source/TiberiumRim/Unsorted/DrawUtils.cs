using UnityEngine;
using Verse;

namespace TiberiumRim;

public static class DrawUtils
{
    public static void DrawBox(Rect rect, float opacity, int thickness)
    {
        DrawBox(rect, new Color(1, 1, 1, opacity), thickness);
    }

    public static void DrawBox(Rect rect, Color color, int thickness)
    {
        var oldColor = GUI.color;
        GUI.color = color;
        Widgets.DrawBox(rect, thickness);
        GUI.color = oldColor;
    }

    public static void DrawColoredBox(Rect rect, Color fillColor, Color borderColor, int thickness)
    {
        var oldColor = GUI.color;
        Widgets.DrawBoxSolid(rect, fillColor);
        GUI.color = borderColor;
        Widgets.DrawBox(rect, thickness);
        GUI.color = oldColor;
    }
}