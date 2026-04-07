using System.Text.RegularExpressions;
using TeleCore.Lib;
using TeleCore.Shared;
using UnityEngine;
using Verse;

namespace TeleCore.TeleUI;

public static partial class TWidgets
{
    public static class Text
    {
        private static Regex HEXREGEX = new Regex("^#([0-9A-Fa-f]{0,6})$");
        
        public static string TextField(Rect rect, string text, int maxLength = -1, Regex? inputValidator = null)
        {
            var ctrlName = $"TeleTextField{rect.y:F0}{rect.x:F0}";
            using var focus = new GUIFocus(rect, ctrlName);
            
            using var context = new GUIStyleContext(GUI.skin.textField, true);
            var result = GUI.TextField(rect, text, maxLength, context.Style);
            if (inputValidator == null || inputValidator.IsMatch(result))
            {
                return result;
            }
            return text;
        }
        
        public static bool TextFieldHex(Rect rect, out string hexResult, string? initialValue = null, string? context = null)
        {
            var ctrlName = $"TextFieldHex{rect.y:F0}{rect.x:F0}";
            using var focus = new GUIFocus(rect, ctrlName);
            //GUIStyle style;
            
            var buffer = UICache.GetBuffer(ctrlName, initialValue ?? "FFFFFF", context);
            var inputRes = GUI.TextField(rect, "#" + buffer, maxLength: 7);
            var match = HEXREGEX.Match(inputRes);
            var result = buffer;
            if (match.Success)
            {
                 result = match.Groups[1].Value;
            }
            UICache.SetBuffer(ctrlName, result, context);
            hexResult = result;
            return hexResult != buffer;
        }

        private static string ReverseFormat(string formatted, string format)
        {
            string pattern = Regex.Escape(format).Replace(@"\{0\}", "(.*)");
            var match = Regex.Match(formatted, pattern);
            return match.Success ? match.Groups[1].Value : formatted;
        }
        
        public static bool TextFieldNumeric<T>(Rect rect, ref T value, float min = 0, float max = float.MaxValue, GUIStyle? style = null, string? context = null) where T : unmanaged
        {
            var ctrlName = $"TextField{rect.y:F0}{rect.x:F0}";
            // ReSharper disable once HeapView.BoxingAllocation
            //TODO: Improve boxing allocation
            var buffer = UICache.GetBuffer(ctrlName, value, context);
            
            using var focus = new GUIFocus(rect, ctrlName);
            if(style != null)
                GUI.skin.textField = style;
            
            var valPrev = value;
            var fieldValue = TextField(rect, buffer);
            
            if (GUI.GetNameOfFocusedControl() != ctrlName)
            {
                Widgets.ResolveParseNow(buffer, ref value, ref buffer, min, max, true);
                return ValueChanged<T>(value, valPrev);
            }
        
            if (fieldValue != buffer && Widgets.IsPartiallyOrFullyTypedNumber<T>(ref value, fieldValue, min, max))
            {
                buffer = fieldValue;
                if (fieldValue.IsFullyTypedNumber<T>() || buffer.Length == 0)
                {
                    Widgets.ResolveParseNow(fieldValue, ref value, ref buffer, min, max, false);
                }
                
                //Update buffer to potential value
                UICache.SetBuffer(ctrlName, buffer, context);
            }
            return ValueChanged<T>(value, valPrev);
        }
        
        public static bool ValueChanged<T>(Numeric<T> value, Numeric<T> value2) where T : unmanaged
        {
            var diff = MathG.Abs(value - value2);
            return diff > Numeric<T>.Epsilon;
        }
    }
}