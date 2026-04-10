using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class TRMats
    {
        public static readonly Texture2D WorkBanner = ContentFinder<Texture2D>.Get("UI/WorkBanner", true);

        //WorldLayer
        public static readonly Material TiberiumInfSmall = MaterialPool.MatFrom("World/TiberiumInf_Small", ShaderDatabase.MoteGlow, 3510);

        public static readonly Material TiberiumInfMedium = MaterialPool.MatFrom("World/TiberiumInf_Medium", ShaderDatabase.MoteGlow);

        public static readonly Material TiberiumInfBig = MaterialPool.MatFrom("World/TiberiumInf_Big", ShaderDatabase.MoteGlow);

        public static readonly Material TiberiumInfLarge = MaterialPool.MatFrom("World/TiberiumInf_Large", ShaderDatabase.MoteGlow);

        public static readonly Material TiberiumGlacier = MaterialPool.MatFrom("World/TiberiumGlacier", ShaderDatabase.WorldOverlayTransparentLit, 3510);

        //Faction Icons
        public static readonly Texture2D GDI = ContentFinder<Texture2D>.Get("UI/Icons/GDI");

        public static readonly Texture2D NOD = ContentFinder<Texture2D>.Get("UI/Icons/Nod");

        public static readonly Texture2D Scrin = ContentFinder<Texture2D>.Get("UI/Icons/Nod");

        //Colors
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
