using System.Xml;
using UnityEngine;
using Verse;

namespace TeleCore.Utils;

public static class TText
{
    public static string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }

    //Text Formatting
    public static string ColorizeFix(this string text, string colorHex)
    {
        return $"<color={colorHex}>{text}</color>";
    }

    public static TaggedString ColorizeFix(this TaggedString text, Color color)
    {
        return ColorizeFix(text, ColorToHex(color));
    }

    public static string ColorizeFix(this string text, Color color)
    {
        return ColorizeFix(text, ColorToHex(color));
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

    public static string ToRefPath(this XmlNode root)
    {
        var val = "";
        var parent = root;
        while (parent.ParentNode != null)
        {
            val = $">{parent.Name}" + val;
            parent = parent.ParentNode;
        }

        return val;
    }
}