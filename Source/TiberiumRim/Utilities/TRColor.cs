using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class TRColor
    {
        public static readonly Color White075 = new Color(1, 1, 1, 0.75f);
        public static readonly Color White05 = new Color(1, 1, 1, 0.5f);
        public static readonly Color White025 = new Color(1, 1, 1, 0.25f);
        public static readonly Color White01 = new Color(1, 1, 1, 0.01f);
        public static readonly Color White005 = new Color(1, 1, 1, 0.05f);

        public static readonly Color LightBlack = new Color(0, 0, 0, 0.15f);
        public static readonly Color GapLineColor = new Color(0.35f, 0.35f, 0.35f);

        public static readonly Color Orange = new ColorInt(255, 175, 0).ToColor;
        public static readonly Color Blue = new ColorInt(38, 169, 224).ToColor;
        public static readonly Color Yellow = new ColorInt(249, 236, 49).ToColor;
        public static readonly Color Red = new ColorInt(190, 30, 45).ToColor;
        public static readonly Color Green = new ColorInt(41, 180, 115).ToColor;
        public static readonly Color Black = new ColorInt(15, 11, 12).ToColor;

        public static readonly Color WindowBGBorderColor = new ColorInt(97, 108, 122).ToColor;
        public static readonly Color WindowBGFillColor = new ColorInt(21, 25, 29).ToColor;
        public static readonly Color MenuSectionBGFillColor = new ColorInt(42, 43, 44).ToColor;
        public static readonly Color MenuSectionBGBorderColor = new ColorInt(135, 135, 135).ToColor;
        public static readonly Color TutorWindowBGFillColor = new ColorInt(133, 85, 44).ToColor;
        public static readonly Color TutorWindowBGBorderColor = new ColorInt(176, 139, 61).ToColor;
        public static readonly Color OptionUnselectedBGFillColor = new Color(0.21f, 0.21f, 0.21f);
        public static readonly Color OptionUnselectedBGBorderColor = OptionUnselectedBGFillColor * 1.8f;
        public static readonly Color OptionSelectedBGFillColor = new Color(0.32f, 0.28f, 0.21f);
        public static readonly Color OptionSelectedBGBorderColor = OptionSelectedBGFillColor * 1.8f;
        public static readonly Color BlueHighlight = new ColorInt(0, 120, 200).ToColor;
        public static readonly Color BlueHighlight_Transparent = new ColorInt(0, 120, 200, 125).ToColor;

        //BGS
        public static readonly Color BGDarker = new ColorInt(29, 30, 30).ToColor;
        public static readonly Color BGLighter = new ColorInt(61, 62, 63).ToColor;

        public static readonly Color BGP3 = new ColorInt(48, 50, 51).ToColor;
        public static readonly Color BGM1 = new ColorInt(39, 40, 40).ToColor;

        //
        public static Color BlueHueBG = new ColorInt(32, 36, 40).ToColor;
        public static Color NiceBlue = new ColorInt(38, 169, 224).ToColor;

        //
        public static Color VisceralColor = new ColorInt(155, 160, 75).ToColor;
        public static Color SymbioticColor = new ColorInt(138, 229, 226).ToColor;
    }
}
