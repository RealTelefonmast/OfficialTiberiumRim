using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using TeleCore.Defs;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class RoomOverlay_Atmospheric : RoomOverlayRenderer
{
    [TweakValue("AE.RoomOverlay_BlendSpeed", 0f, 2f)]
    public static float BlendSpeed = 0.4f;

    [TweakValue("AE.RoomOverlay_BlendValue", 0f, 1f)]
    public static float BlendValue = 0.48f;

    [TweakValue("AE.RoomOverlay_Alpha", 0f, 1f)]
    public static float Alpha = 1f;

    [TweakValue("AE.RoomOverlay_SrcMode", 0f, 10f)]
    public static int SrcMode = 2;

    private readonly Dictionary<AtmosphericValueDef, Material> materialsByDef;

    public RoomOverlay_Atmospheric()
    {
        materialsByDef = new Dictionary<AtmosphericValueDef, Material>();
    }

    public override Shader Shader => TAEUnityContent.TextureBlend;

    public Dictionary<AtmosphericValueDef, Material>.KeyCollection Defs => materialsByDef.Keys;

    public void TryRegisterNewOverlayPart(AtmosphericValueDef valueDef)
    {
        if (valueDef.roomOverlay == null) return;
        if (materialsByDef.ContainsKey(valueDef)) return;
        TeleUpdateManager.Notify_EnqueueNewSingleAction(() => GetMaterial(valueDef));
    }

    public Material GetMaterial(AtmosphericValueDef valueDef)
    {
        if (!materialsByDef.TryGetValue(valueDef, out var mat))
        {
            mat = new Material(TAEUnityContent.TextureBlend);
            InitShaderProps(mat, valueDef);
            materialsByDef.Add(valueDef, mat);
        }

        UpdateShaderProps(mat, valueDef);
        return mat;
    }

    protected void InitShaderProps(Material material, AtmosphericValueDef valueDef)
    {
        TeleUpdateManager.Notify_EnqueueNewSingleAction(() =>
        {
            var overlayPart1 = ContentFinder<Texture2D>.Get(valueDef.roomOverlay.overlayTex1);
            var overlayPart2 = ContentFinder<Texture2D>.Get(valueDef.roomOverlay.overlayTex2);
            material.SetTexture("_MainTex1", overlayPart1);
            material.SetTexture("_MainTex2", overlayPart2);
        });
        var color = valueDef.roomOverlay.color;
        color.a = 100f / 255f;
        material.SetFloat("_SrcMode", 2);
        material.SetFloat("_DstMode", 1);
        material.SetColor("_Color", color);
    }

    public void UpdateTick()
    {
        if (!materialsByDef.Any()) return;
        foreach (var matDef in materialsByDef) UpdateShaderProps(matDef.Value, matDef.Key);
    }

    protected void UpdateShaderProps(Material material, AtmosphericValueDef valueDef)
    {
        base.UpdateShaderProps(material);
        material.SetFloat("_SrcMode", SrcMode);
        material.SetFloat("_BlendValue", BlendValue);
        material.SetFloat("_BlendSpeed", BlendSpeed);
        material.SetFloat("_Opacity", MainAlpha * Alpha);
    }

    public void DrawFor(AtmosphericValueDef valueDef, Vector3 drawPos, float saturation)
    {
        if (cachedMesh == null) return;
        MainAlpha = saturation;

        Matrix4x4 matrix = default;
        matrix.SetTRS(drawPos, Quaternion.identity, Vector3.one);
        Graphics.DrawMesh(cachedMesh, matrix, GetMaterial(valueDef), 0);
    }
}