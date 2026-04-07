using System;
using Unity.Mathematics;
using UnityEngine;
using Verse;

namespace TeleCore.TeleUI;

[Flags]
public enum BorderType : byte
{
    None = 0,
    Top = 1,
    Bottom = 2,
    Left = 4,
    Right = 8,
    All = 15,
}

public static class TUI
{
    public static void DoControl(Rect rect)
    {
        var id = GUIUtility.GetControlID(FocusType.Passive, rect);
        
    }

    public static void DrawBorder(Rect rect, Color color, int thickness = 2, BorderType border = BorderType.All)
    {
        var ev = Event.current;
        switch (ev.type)
        {
            case EventType.Repaint:
            {
                var offset = thickness / 2;
                rect.ExpandedBy(-offset, -offset);
                if ((border & BorderType.Top) == BorderType.Top)
                {
                    var from = new float2(rect.x-offset, rect.y);
                    var to = new float2(rect.x + rect.width + offset, rect.y);
                    DrawLine(from, to, color, thickness);  
                }

                if ((border & BorderType.Left) == BorderType.Left)
                {
                    var from = new float2(rect.x, rect.y - offset);
                    var to = new float2(rect.x, rect.y + rect.height + offset);
                    DrawLine(from, to, color, thickness);  
                }

                if ((border & BorderType.Right) == BorderType.Right)
                {
                    var from = new float2(rect.x + rect.width, rect.y - offset);
                    var to = new float2(rect.x + rect.width, rect.y + rect.height + offset);
                    DrawLine(from, to, color, thickness);  
                }

                if ((border & BorderType.Bottom) == BorderType.Bottom)
                {
                    var from = new float2(rect.x - offset, rect.y + rect.height);
                    var to = new float2(rect.x + rect.width + offset, rect.y + rect.height);
                    DrawLine(from, to, color, thickness);  
                }
            }
            break;
        }
    }
    
    public static void DrawLine(float2 from, float2 to, Color color, int lineWidth = 2)
    {
        var x1 = from.x;
        var y1 = from.y;
        var x2 = to.x;
        var y2 = to.y;

        var angle = (float)Math.Atan2(y2 - y1, x2 - x1); // Angle calculation 
        var length = Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2)); // Length calculation

        var rectangle = new Rect(x1, y1, (int)length, lineWidth);
        //var origin = new Vector2(0, 0.5f); // We want to rotate around the center of the line 

        BeginRotated(angle, rectangle.center);
        GUI.DrawTexture(rectangle, BaseContent.WhiteTex);
        EndRotated();
    }

    private static Matrix4x4 _lastState;
    
    public static void BeginRotated(float angle, float2 around)
    {
        _lastState = GUI.matrix;
        Verse.UI.RotateAroundPivot(angle, around);
    }
    
    public static void EndRotated()
    {
        GUI.matrix = _lastState;
    }
    
    public static void DrawLayout(Action<Rect> layout, Action<Rect> paint)
    {
        
    }

    internal static void CheckEndGUIStack()
    {
        
    }
}

public static class GUIStateStack
{
    public static void BeginSate()
    {
        
    }

    public static void EndState()
    {
        
    }
}