using UnityEngine;
using Verse;

namespace TeleCore.Utility.Serialization;

public static class Scribe_Rect
{
    public static void Look(ref Rect value, string label, Rect defaultValue = default)
    {
        defaultValue = Rect.zero;
        if (Scribe.mode == LoadSaveMode.Saving) Scribe.saver.WriteElement(label, value.ToStringSimple());

        if (Scribe.mode == LoadSaveMode.LoadingVars)
            value = ScribeExtractor.ValueFromNode(Scribe.loader.curXmlParent[label], defaultValue);
    }

    public static string ToStringSimple(this Rect rect)
    {
        return $"({rect.x},{rect.y},{rect.width},{rect.height})";
    }
}