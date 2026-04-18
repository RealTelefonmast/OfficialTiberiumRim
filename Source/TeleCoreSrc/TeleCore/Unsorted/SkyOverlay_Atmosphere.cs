using LudeonTK;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace TeleCore.Unsorted;

public abstract class SkyOverlay_Atmosphere : SkyOverlay
{
    [TweakValue("AE.SkyOverlay_Tiling", 0.25f, 4f)]
    public static float Tiling = 0.34f;

    [TweakValue("AE.SkyOverlay_Opacity", 0f, 1f)]
    public static float Opacity = 1;

    [TweakValue("AE.SkyOverlay_ASrcMode", 0f, 10f)]
    public static int SrcMode = 2;

    [TweakValue("AE.SkyOverlay_BDstMode", 0f, 10f)]
    public static int DstMode = 7;

    private Color initColor = Color.white;

    public SkyOverlay_Atmosphere(NaturalOverlayProperties props)
    {
        CreateCopy();
        if (props.overlayTex?.Length > 0)
        {
            Material.SetTexture("_MainTex", ContentFinder<Texture2D>.Get(props.overlayTex));

            if (props.overlayTex2?.Length > 0)
                Material.SetTexture("_MainTex2", ContentFinder<Texture2D>.Get(props.overlayTex2));
        }

        //
        SetupData();
        SetColor(props.color);
        SetScale(props.scale);
    }

    private Material Material { get; set; }

    private void CreateCopy()
    {
        Material = TAEUnityContent.CustomOverlayWorld;
        SrcMode = (int)Material.GetFloat("_SrcMode");
        DstMode = (int)Material.GetFloat("_DstMode");

        var color = Material.GetColor("_Color");
        TLog.Message($"Init Color: {ColorInt(color)}");

        TLog.Message($"Loaded Mat: {Material} with shader: {Material.shader.name}");
    }

    private void SetupData()
    {
        Material = Material;
        speed = 0.0002f;
        worldPanDir1 = new Vector2(0.25f, 0.75f);
        worldPanDir1.Normalize();
        worldOverlayPanSpeed2 = 0.00015f;
        worldPanDir2 = new Vector2(0.20f, 0.70f);
        worldPanDir2.Normalize();
    }

    public void SetColor(Color color)
    {
        TLog.Message($"Setting color: {color}");
        var color1 = Material.GetColor("_Color");
        Material.SetColor("_Color", color);
        var color2 = Material.GetColor("_Color");
        initColor = color;
        TLog.Message($"New Color: {color1} -> {color2}");
    }

    public override void TickOverlay(Map map)
    {
        //base.TickOverlay(map);
        SetScale(new Vector2(Tiling, Tiling));
        SetBlendMode((BlendMode)SrcMode, (BlendMode)DstMode);

        initColor.a = Opacity;
        Material.SetColor("_Color", initColor);
    }

    public string ColorInt(Color color)
    {
        var cint = new ColorInt(color);
        return $"({cint.r}, {cint.g}, {cint.b}, {cint.a})";
    }

    public void SetBlendMode(BlendMode source, BlendMode destination)
    {
        Material.SetFloat("_SrcMode", (int)source);
        Material.SetFloat("_DstMode", (int)destination);
    }

    public void SetScale(Vector2 scale)
    {
        Material.SetTextureScale("_MainTex", scale);
        Material.SetTextureScale("_MainTex2", scale * 0.85f);
    }
}