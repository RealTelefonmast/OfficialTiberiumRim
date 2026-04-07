using System.Collections.Generic;
using UnityEngine;

namespace TeleCore.DDGUI;

public struct UIMenu
{
    private bool _isToggled;
    
    public  void Test()
    {
         _isToggled = GUI.Toggle(Rect.zero, _isToggled, GUIContent.none);
            
    }

    private static Dictionary<int, bool> _toggleRefs;

    public static bool ToggleButton(Rect rect, out bool wasToggled)
    {
        int id = GUIUtility.GetControlID(rect.GetHashCode(), FocusType.Passive, rect);
        var previous = _toggleRefs[id];

        bool toggle = GUI.Toggle(rect, previous, GUIContent.none);
        _toggleRefs[id] = toggle;

        wasToggled = previous != toggle;
        return toggle;
    }
}