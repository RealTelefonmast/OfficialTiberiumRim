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
        public static readonly Color White005 = new Color(1, 1, 1, 0.05f);

        public static readonly Color LightBlack = new Color(0, 0, 0, 0.15f);

        public static readonly Color GapLineColor = new Color(0.35f, 0.35f, 0.35f);

        public static readonly Color Orange = new ColorInt(255, 175, 0).ToColor;
        public static readonly Color Blue = new ColorInt(38, 169, 224).ToColor;
        public static readonly Color Yellow = new ColorInt(249, 236, 49).ToColor;
        public static readonly Color Red = new ColorInt(190, 30, 45).ToColor;
        public static readonly Color Green = new ColorInt(41, 180, 115).ToColor;
        public static readonly Color Black = new ColorInt(15, 11, 12).ToColor;

        //
        public static Color VisceralColor = new ColorInt(155, 160, 75).ToColor;
        public static Color SymbioticColor = new ColorInt(138, 229, 226).ToColor;
    }
}
