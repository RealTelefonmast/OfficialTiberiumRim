using System;
using UnityEngine;

namespace TeleCore.Unsorted;

public class GUIFocus : IDisposable
{
    private readonly string controlName;
    private readonly Vector2 mousePos;
    private Rect focusRect;

    public GUIFocus(Rect focusRect, string controlName)
    {
        this.focusRect = focusRect;
        this.controlName = controlName;
        mousePos = Event.current.mousePosition;
        GUI.SetNextControlName(controlName);
    }

    public void Dispose()
    {
        var clicked = Event.current.isMouse && Event.current.button == 0;
        var mousePos = Event.current.mousePosition;
        var newPos = GUIUtility.ScreenToGUIPoint(mousePos);

        var inRect1 = focusRect.Contains(this.mousePos);
        var inRect2 = focusRect.Contains(newPos);
        if (GUI.GetNameOfFocusedControl() == controlName && clicked && !inRect2) GUI.FocusControl(null);
    }
}