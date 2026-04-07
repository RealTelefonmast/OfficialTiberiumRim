using UnityEngine;
using Verse;

namespace TeleCore.TeleUI;

public static partial class TWidgets
{
    #region Grouping

    public static void BeginGroup(Rect position)
    {
        BeginGroup(position, GUIContent.none, GUIStyle.none);
    }

    public static void BeginGroup(Rect position, string text)
    {
        BeginGroup(position, text, GUIStyle.none);
    }

    public static void BeginGroup(Rect position, Texture image)
    {
        BeginGroup(position, image, GUIStyle.none);
    }

    public static void BeginGroup(Rect position, GUIContent content)
    {
        BeginGroup(position, content, GUIStyle.none);
    }

    public static void BeginGroup(Rect position, GUIStyle style)
    {
        BeginGroup(position, GUIContent.none, style);
    }

    public static void BeginGroup(Rect position, string text, GUIStyle style)
    {
        GUI.BeginGroup(position, text, style);
        UnityGUIBugsFixer.Notify_BeginGroup();
    }

    public static void BeginGroup(Rect position, Texture image, GUIStyle style)
    {
        GUI.BeginGroup(position, image, style);
        UnityGUIBugsFixer.Notify_BeginGroup();
    }

    public static void BeginGroup(Rect position, GUIContent content, GUIStyle style)
    {
        GUI.BeginGroup(position, content, style);
        UnityGUIBugsFixer.Notify_BeginGroup();
    }
    
    public static void EndGroup()
    {
        GUI.EndGroup();
        UnityGUIBugsFixer.Notify_EndGroup();
    }
    
    #endregion
}