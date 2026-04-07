using System;
using UnityEngine;

namespace TeleCore.TeleUI;

public class GUIFocus : IDisposable
{
    private Rect focusRect;
    private string controlName;
    private Vector2 mousePos;
    
    public GUIFocus(Rect focusRect, string controlName)
    {
        this.focusRect = focusRect;
        this.controlName = controlName;
        this.mousePos = Event.current.mousePosition;
        GUI.SetNextControlName(controlName);
    }
    
    public void Dispose()
    {
        var clicked = Event.current.isMouse && Event.current.button == 0;
        var mousePos = Event.current.mousePosition;
        var newPos = GUIUtility.ScreenToGUIPoint(mousePos);
        
        var inRect1 = focusRect.Contains(this.mousePos);
        var inRect2 = focusRect.Contains(newPos);
        if (GUI.GetNameOfFocusedControl() == controlName && clicked && !inRect2)
        {
            GUI.FocusControl(null);
        }
    }
}