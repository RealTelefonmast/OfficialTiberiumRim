using UnityEngine;
using Verse;

namespace TeleCore.Atmospheres.Types.Utils;

[StaticConstructorOnStartup]
public static class AtmosContent
{
    public static Texture2D AtmosphereIcon = ContentFinder<Texture2D>.Get("UI/Icons/AtmospherePlaySetting");
    public static Texture2D SpreadingGasBase = ContentFinder<Texture2D>.Get("Sprites/Gas");

    internal static readonly Material FilledMat =
        SolidColorMaterials.NewSolidColorMaterial(Color.green, ShaderDatabase.MetaOverlay);

    internal static readonly Material UnFilledMat =
        SolidColorMaterials.NewSolidColorMaterial(TColor.LightBlack, ShaderDatabase.MetaOverlay);
}