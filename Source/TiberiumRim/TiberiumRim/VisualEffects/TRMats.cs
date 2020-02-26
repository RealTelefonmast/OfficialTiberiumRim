using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class TRMats
    {
        public static readonly Color GapLineColor = new Color(0.35f, 0.35f, 0.35f);

        public static readonly Texture2D DarkGreyBG = SolidColorMaterials.NewSolidColorTexture(new Color(0.15f, 0.15f, 0.15f));
        public static readonly Texture2D GreenType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.GreenColor);
        public static readonly Texture2D BlueType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.BlueColor);
        public static readonly Texture2D RedType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.RedColor);
        public static readonly Texture2D GasType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.GasColor);
        public static readonly Texture2D SludgeType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.SludgeColor);
        public static readonly Texture2D EmptyContainer = ContentFinder<Texture2D>.Get("UI/Icons/ConnectionMissing", false);
        public static readonly Texture2D MarkedForDeath = ContentFinder<Texture2D>.Get("UI/Icons/Marked", false);

        public static readonly Texture2D mutationVisceral = SolidColorMaterials.NewSolidColorTexture(new ColorInt(155, 160, 75).ToColor);
        public static readonly Texture2D mutationGreen = SolidColorMaterials.NewSolidColorTexture(new ColorInt(175, 255, 0).ToColor);
        public static readonly Texture2D mutationBlue = SolidColorMaterials.NewSolidColorTexture(new ColorInt(138, 229, 226).ToColor);

        public static readonly Texture2D clear = SolidColorMaterials.NewSolidColorTexture(Color.clear);
        public static readonly Texture2D grey = SolidColorMaterials.NewSolidColorTexture(Color.grey);
        public static readonly Texture2D blue = SolidColorMaterials.NewSolidColorTexture(new ColorInt(38, 169, 224).ToColor);
        public static readonly Texture2D yellow = SolidColorMaterials.NewSolidColorTexture(new ColorInt(249, 236, 49).ToColor);
        public static readonly Texture2D red = SolidColorMaterials.NewSolidColorTexture(new ColorInt(190, 30, 45).ToColor);
        public static readonly Texture2D green = SolidColorMaterials.NewSolidColorTexture(new ColorInt(41, 180, 115).ToColor);
        public static readonly Texture2D white = SolidColorMaterials.NewSolidColorTexture(new ColorInt(255, 255, 255).ToColor);
        public static readonly Texture2D black = SolidColorMaterials.NewSolidColorTexture(new ColorInt(15, 11, 12).ToColor);
    }
}
