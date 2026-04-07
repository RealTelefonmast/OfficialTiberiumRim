using UnityEngine;
using Verse;

namespace TeleCore.Shared;

public static class TColor
{
    //UI
    //BGs
    public static readonly Color BGDarker = new(29 / 255f, 30 / 255f, 30 / 255f);
    public static readonly Color BGLighter = new(61 / 255f, 62 / 255f, 63 / 255f);

    public static readonly Color BlueHueBG = new(32 / 255f, 36 / 255f, 40 / 255f);

    public static readonly Color BGP3 = new(48 / 255f, 50 / 255f, 51 / 255f);
    public static readonly Color BGM1 = new(39 / 255f, 40 / 255f, 40 / 255f);

    //Special
    public static Color NiceBlue = new(0.17f, 0.74f, 1f);

    //Cached Whites
    public static readonly Color White075 = new(1, 1, 1, 0.75f);
    public static readonly Color White05 = new(1, 1, 1, 0.5f);
    public static readonly Color White025 = new(1, 1, 1, 0.25f);
    public static readonly Color White01 = new(1, 1, 1, 0.10f);
    public static readonly Color White005 = new(1, 1, 1, 0.05f);

    public static readonly Color LightBlack = new(0, 0, 0, 0.15f);
    public static readonly Color GapLineColor = new(0.35f, 0.35f, 0.35f);

    //Extra Colors
    public static readonly Color Orange = new(1, 0.5f, 0);
    public static readonly Color Blue = new(0.15f, 0.66f, 0.88f);
    public static readonly Color Yellow = new(0.97f, 0.92f, 0.17f);
    public static readonly Color Red = new(0.74f, 0.1f, 0.15f);
    public static readonly Color Purple = new(0.53f, 0f, 0.53f);
    public static readonly Color Green = new(0.16f, 0.71f, 0.45f);
    public static readonly Color Black = new(0.06f, 0.04f, 0f);

    //Vanilla Copies
    public static readonly Color WindowBGBorderColor = new ColorInt(97, 108, 122).ToColor;
    public static readonly Color WindowBGFillColor = new ColorInt(21, 25, 29).ToColor;
    public static readonly Color MenuSectionBGFillColor = new ColorInt(42, 43, 44).ToColor;
    public static readonly Color MenuSectionBGBorderColor = new ColorInt(135, 135, 135).ToColor;
    public static readonly Color TutorWindowBGFillColor = new ColorInt(133, 85, 44).ToColor;
    public static readonly Color TutorWindowBGBorderColor = new ColorInt(176, 139, 61).ToColor;
    public static readonly Color OptionUnselectedBGFillColor = new(0.21f, 0.21f, 0.21f);
    public static readonly Color OptionUnselectedBGBorderColor = OptionUnselectedBGFillColor * 1.8f;
    public static readonly Color OptionSelectedBGFillColor = new(0.32f, 0.28f, 0.21f);
    public static readonly Color OptionSelectedBGBorderColor = OptionSelectedBGFillColor * 1.8f;
    public static readonly Color BlueHighlight = new ColorInt(0, 120, 200).ToColor;
    public static readonly Color BlueHighlight_Transparent = new ColorInt(0, 120, 200, 125).ToColor;
}