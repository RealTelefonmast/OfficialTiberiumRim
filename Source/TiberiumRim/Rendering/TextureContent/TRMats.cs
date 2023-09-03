﻿using UnityEngine;
using Verse;

namespace TR
{
    [StaticConstructorOnStartup]
    public class TRMats
    {
        public static readonly Texture2D DarkGreyBG = SolidColorMaterials.NewSolidColorTexture(new Color(0.15f, 0.15f, 0.15f));
        public static readonly Texture2D GreenType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.GreenColor);
        public static readonly Texture2D BlueType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.BlueColor);
        public static readonly Texture2D RedType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.RedColor);
        public static readonly Texture2D GasType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.GasColor);
        public static readonly Texture2D SludgeType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.SludgeColor);

        public static readonly Texture2D MutationVisceral = SolidColorMaterials.NewSolidColorTexture(TRColor.VisceralColor);
        public static readonly Texture2D MutationSymbiotic = SolidColorMaterials.NewSolidColorTexture(TRColor.SymbioticColor);
        public static readonly Texture2D MutationGreen = SolidColorMaterials.NewSolidColorTexture(new ColorInt(175, 255, 0).ToColor);

        public static readonly Texture2D clear = SolidColorMaterials.NewSolidColorTexture(Color.clear);
        public static readonly Texture2D grey = SolidColorMaterials.NewSolidColorTexture(Color.grey);
        public static readonly Texture2D blue = SolidColorMaterials.NewSolidColorTexture(TRColor.Blue);
        public static readonly Texture2D yellow = SolidColorMaterials.NewSolidColorTexture(TRColor.Yellow);
        public static readonly Texture2D red = SolidColorMaterials.NewSolidColorTexture(TRColor.Red);
        public static readonly Texture2D green = SolidColorMaterials.NewSolidColorTexture(TRColor.Green);
        public static readonly Texture2D white = SolidColorMaterials.NewSolidColorTexture(Color.white);
        public static readonly Texture2D black = SolidColorMaterials.NewSolidColorTexture(TRColor.Black);
    }
}
