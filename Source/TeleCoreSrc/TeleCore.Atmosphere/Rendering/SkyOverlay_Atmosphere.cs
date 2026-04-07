using LudeonTK;
using TeleCore.Atmosphere.Defs;
using TeleCore.Atmosphere.Static;
using TeleCore.Logging;
using TeleCore.Rendering;
using TeleCore.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace TeleCore.Atmosphere.Rendering;

public abstract class SkyOverlay_Atmosphere : SkyOverlay
{
    private Material materialInt;
    private Material Material => materialInt;

    [TweakValue("AE.SkyOverlay_Tiling", 0.25f, 4f)]
    public static float Tiling = 0.34f;

    [TweakValue("AE.SkyOverlay_Opacity", 0f, 1f)]
    public static float Opacity = 1;

    [TweakValue("AE.SkyOverlay_ASrcMode", 0f, 10f)]
    public static int SrcMode = 2;
    [TweakValue("AE.SkyOverlay_BDstMode", 0f, 10f)]
    public static int DstMode = 7;

    public SkyOverlay_Atmosphere(NaturalOverlayProperties props)
    {
        CreateCopy();
        if (props.overlayTex?.Length > 0)
        {
            materialInt.SetTexture("_MainTex", ContentFinder<Texture2D>.Get(props.overlayTex));

            if (props.overlayTex2?.Length > 0)
                materialInt.SetTexture("_MainTex2", ContentFinder<Texture2D>.Get(props.overlayTex2));
        }

        //
        SetupData();
        SetColor(props.color);
        SetScale(props.scale);
    }

    private Color initColor = Color.white;
    private void CreateCopy()
    {
        materialInt = TAEUnityContent.CustomOverlayWorld;
        SrcMode = (int) materialInt.GetFloat("_SrcMode");
        DstMode = (int) materialInt.GetFloat("_DstMode");

        var color = materialInt.GetColor("_Color");
        TLog.Message($"Init Color: {ColorInt(color)}");

        TLog.Message($"Loaded Mat: {materialInt} with shader: {materialInt.shader.name}");
    }

    private void SetupData()
    {
        worldOverlayMat = Material;
        worldOverlayPanSpeed1 = 0.0002f;
        worldPanDir1 = new Vector2(0.25f, 0.75f);
        worldPanDir1.Normalize();
        worldOverlayPanSpeed2 = 0.00015f;
        worldPanDir2 = new Vector2(0.20f, 0.70f);
        worldPanDir2.Normalize();
    }

    public void SetColor(Color color)
    {
        TLog.Message($"Setting color: {color}");
        var color1 = materialInt.GetColor("_Color");
        materialInt.SetColor("_Color", color);
        var color2 = materialInt.GetColor("_Color");
        initColor = color;
        TLog.Message($"New Color: {color1} -> {color2}");
    }

    public override void TickOverlay(Verse.Map map)
    {
        //base.TickOverlay(map);
        SetScale(new Vector2(Tiling, Tiling));
        SetBlendMode((BlendMode) SrcMode, (BlendMode) DstMode);

        initColor.a = Opacity;
        materialInt.SetColor("_Color", initColor);
    }

    public string ColorInt(Color color)
    {
        var cint = new ColorInt(color);
        return $"({cint.r}, {cint.g}, {cint.b}, {cint.a})";
    }

    public void SetBlendMode(BlendMode source, BlendMode destination)
    {
        materialInt.SetFloat("_SrcMode", (int) source);
        materialInt.SetFloat("_DstMode", (int) destination);
    }

    public void SetScale(Vector2 scale)
    {
        materialInt.SetTextureScale("_MainTex", scale);
        materialInt.SetTextureScale("_MainTex2", scale * 0.85f);
    }
}
