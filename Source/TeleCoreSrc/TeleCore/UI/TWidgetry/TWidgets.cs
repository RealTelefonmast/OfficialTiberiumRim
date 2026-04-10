using RimWorld;
using UnityEngine;
using Verse;
using VerseUI = Verse.UI;

namespace TeleCore.TeleUI;

public static partial class TWidgets
{
    #region World & Screen Coords
    
    public static Vector2 ToScreenPos(this Vector3 vec)
    {
        Vector2 vector = Find.Camera.WorldToScreenPoint(vec) / Prefs.UIScale;
        vector.y = VerseUI.screenHeight - vector.y;
        return vector;
    }
    
    public static float PixelWidth(int cellWidth)
    {
        Vector3 point1 = new Vector3(cellWidth, 0, 0);
        Vector3 point2 = Vector3.zero;
        Vector3 point1_screen = Find.Camera.WorldToScreenPoint(point1) / Prefs.UIScale;
        Vector3 point2_screen = Find.Camera.WorldToScreenPoint(point2) / Prefs.UIScale;
        float pixelWidth = (point1_screen - point2_screen).magnitude;
        return pixelWidth;
    }

    #endregion
    
    public static void DrawBoxOnThing(Thing thing)
    {
        var v = ToScreenPos(thing.TrueCenter());

        var driver = Find.CameraDriver;
        var size = 0.5f * driver.CellSizePixels;
        var sizeHalf = size * 0.5f;

        var rect = new Rect(v.x - sizeHalf, v.y - sizeHalf, size, size);
        DrawColoredBox(rect, new Color(1, 1, 1, 0.1f), Color.white, 1);

        if (VerseUI.MousePositionOnUIInverted.DistanceToRect(rect) < 40)
            GenMapUI.DrawThingLabel(rect.position + (rect.size / 2), thing.ToString(), Color.white);
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