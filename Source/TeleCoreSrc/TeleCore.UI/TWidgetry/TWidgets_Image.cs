using System;
using TeleCore.TeleUI.UITypes;
using UnityEngine;
using Verse;

namespace TeleCore.TeleUI;

public static partial class TWidgets
{
    public static class Image
    {
        public static bool DrawTextureWithSlider(Rect rect, UIMaterial material, ref float sliderVal, float min = 0,
            float max = 1, Color? sliderColor = null)
        {
            var id = GUIUtility.GetControlID(FocusType.Passive, rect);

            var previous = sliderVal;
            var mousePos = Event.current.mousePosition;

            var curEv = Event.current.GetTypeForControl(id);

            switch (curEv)
            {
                case EventType.Repaint:
                {
                    var sliderPosX = rect.x + rect.width * Mathf.InverseLerp(min, max, sliderVal);
                    var sliderRect =
                        new Rect(sliderPosX - 1, rect.y - 1, 3,
                            rect.height + 2); //new Rect(sliderPosX-2, rect.y - 2, 6, rect.height + 4);
                    var selectionArea = rect.ExpandedBy(2);
                    var isFocusedOrHovered = GUIUtility.hotControl == id | selectionArea.Contains(mousePos);
                    var color = isFocusedOrHovered ? Color.white : Color.grey;
                    Widgets.DrawBoxSolid(rect.ExpandedBy(1), color);
                    Graphics.DrawTexture(rect, material.Texture, 0, 0, 0, 0, material.Material);
                    Widgets.DrawBoxSolid(sliderRect, Color.white);
                    //Widgets.DrawBoxSolidWithOutline(sliderRect, sliderColor ?? Color.white, Color.black);
                    break;
                }
                case EventType.MouseDown:
                {
                    if (rect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                        GUIUtility.hotControl = id;
                    break;
                }
                case EventType.MouseUp:
                {
                    if (GUIUtility.hotControl == id)
                        GUIUtility.hotControl = 0;
                    break;
                }
                case EventType.MouseDrag:
                {
                    if (GUIUtility.hotControl == id)
                    {
                        var newPos = mousePos.x - rect.x;
                        var newSliderVal = Mathf.InverseLerp(0, rect.width, newPos);
                        sliderVal = Mathf.Lerp(min, max, newSliderVal);
                        GUI.changed = true;
                        Event.current.Use();
                    }

                    break;
                }

            }
            return Math.Abs(previous - sliderVal) > float.Epsilon;
        }
    }
}