using UnityEngine;
using Verse;

namespace TR;

public static class TiberiumMats
{
    public static readonly Texture2D GreenType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.GreenColor);
    public static readonly Texture2D BlueType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.BlueColor);
    public static readonly Texture2D RedType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.RedColor);
    public static readonly Texture2D GasType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.GasColor);
    public static readonly Texture2D SludgeType = SolidColorMaterials.NewSolidColorTexture(MainTCD.Main.SludgeColor);
}