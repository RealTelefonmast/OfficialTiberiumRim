using System.Collections.Generic;
using RimWorld;
using TeleCore.Types;
using TeleCore.Visual.VFX.FX.Layer.Properties;
using UnityEngine;
using Verse;
using FadeProperties = TeleCore.Types.FadeProperties;
using ResizeProperties = TeleCore.Types.ResizeProperties;
using RotateProperties = TeleCore.Types.RotateProperties;

namespace TeleCore.Visual.VFX.FX.Layer;

public class FXLayerData
{
    internal const string _ThingHolderTag = "FXParentThing";
    internal const string _NetworkHolderTag = "FXNetwork";

    public AltitudeLayer? altitude = null;
    public BlinkProperties? blink;
    public string categoryTag;
    public int? drawLayer = null;
    public FadeProperties? fade;
    public FXMode fxMode = FXMode.Static;

    //Main graphic
    public GraphicData? graphicData;
    public string layerTag;
    public bool needsPower = false;
    public Vector3? pivotOffset;
    public Vector3? pivotPixelOffset = null;

    //
    public int? renderPriority; //Otherwise set by index
    public ResizeProperties? resize;

    //
    public RotateProperties? rotate;
    public bool skip = false;

    //Texture UV Data
    public Rect texCoords = new(0, 0, 1, 1);
    public List<DynamicTextureParameter> textureParams;
    public Vector2 textureSize = Vector2.one;

    //public List<EffecterDef> effecters;

    public Vector3? PivotOffset
    {
        get
        {
            if (pivotOffset != null) return pivotOffset;
            if (pivotPixelOffset != null)
            {
                var pixelOffset = pivotPixelOffset.Value;

                var width = pixelOffset.x / textureSize.x * graphicData.drawSize.x;
                var height = pixelOffset.z / textureSize.y * graphicData.drawSize.y;

                pivotOffset = new Vector3(width, 0, height);
            }

            return pivotOffset;
        }
    }

    public void PostLoad()
    {
        if (graphicData != null)
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                graphicData.shaderType ??= ShaderTypeDefOf.Cutout;
                graphicData.Init();
            });
    }
}