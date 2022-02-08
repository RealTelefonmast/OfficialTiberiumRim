using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class TRText
    {
        public static string ColorToHex(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }

        //Text Formatting
        public static TaggedString Colorize(this string text, string colorHex)
        {
            return $"<color={colorHex}>{text}</color>";
        }

        public static TaggedString Colorize(this TaggedString text, Color color)
        {
            return Colorize(text, ColorToHex(color));
        }

        public static TaggedString Colorize(this string text, Color color)
        {
            return Colorize(text, ColorToHex(color));
        }

        public static TaggedString Bold(this TaggedString text)
        {
            return $"<b>{text}</b>";
        }

        public static TaggedString Bold(this string text)
        {
            return $"<b>{text}</b>";
        }

        public static TaggedString Italic(this TaggedString text)
        {
            return $"<i>{text}</i>";
        }

        public static TaggedString Italic(this string text)
        {
            return $"<i>{text}</i>";
        }

        public static TaggedString StrikeThrough(this string text)
        {
            return $"<s>{text}</s>";
        }

    }
}
