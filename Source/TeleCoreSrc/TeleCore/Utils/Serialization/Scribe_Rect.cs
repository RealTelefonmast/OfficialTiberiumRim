using UnityEngine;
using Verse;

namespace TeleCore.Utils.Serialization;

public static class Scribe_Rect
{
    public static void Look(ref Rect value, string label, Rect defaultValue = default)
    {
        defaultValue = Rect.zero;
        if (Verse.Scribe.mode == LoadSaveMode.Saving) Verse.Scribe.saver.WriteElement(label, value.ToStringSimple());

        if (Verse.Scribe.mode == LoadSaveMode.LoadingVars)
            value = ScribeExtractor.ValueFromNode(Verse.Scribe.loader.curXmlParent[label], defaultValue);
    }

    public static string ToStringSimple(this Rect rect)
    {
        return $"({rect.x},{rect.y},{rect.width},{rect.height})";
    }
}