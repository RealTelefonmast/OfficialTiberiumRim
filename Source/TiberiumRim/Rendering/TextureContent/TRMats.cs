using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class TRMats
    {
        public static readonly Color WindowBGBorderColor           = new ColorInt(97, 108, 122).ToColor;
        public static readonly Color WindowBGFillColor             = new ColorInt(21, 25, 29).ToColor;
        public static readonly Color MenuSectionBGFillColor        = new ColorInt(42, 43, 44).ToColor;
        public static readonly Color MenuSectionBGBorderColor      = new ColorInt(135, 135, 135).ToColor;
        public static readonly Color TutorWindowBGFillColor        = new ColorInt(133, 85, 44).ToColor;
        public static readonly Color TutorWindowBGBorderColor      = new ColorInt(176, 139, 61).ToColor;
        public static readonly Color OptionUnselectedBGFillColor   = new Color(0.21f, 0.21f, 0.21f);
        public static readonly Color OptionUnselectedBGBorderColor = OptionUnselectedBGFillColor * 1.8f;
        public static readonly Color OptionSelectedBGFillColor     = new Color(0.32f, 0.28f, 0.21f);
        public static readonly Color OptionSelectedBGBorderColor   = OptionSelectedBGFillColor * 1.8f;
        public static readonly Color BlueHighlight                 = new ColorInt(0, 120, 200).ToColor;
        public static readonly Color BlueHighlight_Transparent = new ColorInt(0, 120, 200, 125).ToColor;

        public static readonly Color BGDarker = new ColorInt(29, 30, 30).ToColor;
        public static readonly Color BGLighter = new ColorInt(61, 62, 63).ToColor;

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
