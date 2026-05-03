using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public static class DebugCellRenderer
{
    private static readonly Dictionary<(Color, Color), Material[]> SpectrumByRange = new();

    //
    public static void RenderCell(IntVec3 cell, Color minColor, Color maxColor, float value)
    {
        CellRenderer.RenderCell(cell, MatFromColorPct(minColor, maxColor, value));
    }

    private static Material MatFromColorPct(Color minColor, Color maxColor, float colorPct)
    {
        var index = Mathf.RoundToInt(Mathf.Lerp(0, 99, colorPct));
        var spectrum = GetSpectrumFor(minColor, maxColor);
        return spectrum[index];
    }

    private static Material[] GetSpectrumFor(Color minColor, Color maxColor)
    {
        //
        if (SpectrumByRange.TryGetValue((minColor, maxColor), out var spectrum)) return spectrum;

        //
        SetupSpectrumFor(minColor, maxColor);
        return GetSpectrumFor(minColor, maxColor);
    }

    private static void SetupSpectrumFor(Color minColor, Color maxColor)
    {
        var key = (minColor, maxColor);
        SpectrumByRange[key] = new Material[100];

        for (var i = 0; i < 100; i++)
        {
            var color = Color.Lerp(minColor, maxColor, i / 100f);
            color.a = 0.25f;
            SpectrumByRange[key][i] = SolidColorMaterials.NewSolidColorMaterial(color, ShaderDatabase.MetaOverlay);
            ;
        }
    }
}