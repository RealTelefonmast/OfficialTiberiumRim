using System;
using System.Linq;
using TeleCore.Comps;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class FXGraphic
{
    private readonly MaterialPropertyBlock materialProperties = new();
    private readonly CompFX parent;
    private readonly Material ShaderMaterial;

    private readonly bool unused;
    public float altitude;
    public int blinkDuration;
    public FXGraphicData data;
    private Color drawColor;

    //Unsaved data
    private GraphicDrawInfo drawInfo;
    private Material drawMat;
    private float exactRotation;
    public Graphic graphicInt;
    public int index;
    private Matrix4x4 matrix;
    private float opacityFloat;
    private float sizeFloat;
    public int ticksToBlink;

    public FXGraphic(CompFX parent, FXGraphicData data, int index)
    {
        //Log.Message(index + " '" + data.data?.texPath + "'" + " which is " + data.mode);
        this.parent = parent;
        this.data = data;
        this.index = index;
        if (data.skip)
        {
            unused = true;
            return;
        }

        if (data.directAltitudeOffset.HasValue)
            altitude = parent.parent.def.altitudeLayer.AltitudeFor() + data.directAltitudeOffset.Value;
        else if (data.altitude.HasValue)
            altitude = data.altitude.Value.AltitudeFor();
        else
            altitude = parent.parent.def.altitudeLayer.AltitudeFor() + 0.125f * (index + 1);
        altitude += data.extraAltitude;
        ShaderMaterial = new Material(TiberiumContent.AlphaShaderMaterial);
    }

    //TODO: Reduce creation of new Graphic instances, go low level unity rendering.
    public Graphic Graphic
    {
        get
        {
            Color color = parent.ColorOverride(index);
            color = color == Color.white ? data.data.color : color;
            var size = data.data.drawSize;
            if (graphicInt == null)
            {
                if (parent.parent.Graphic is Graphic_Random random)
                {
                    var path = data.data.texPath;
                    var parentName = random.SubGraphicFor(parent.parent).path.Split('/').Last();
                    var lastPart = path.Split('/').Last();
                    path += "/" + lastPart;
                    path += "_" + parentName.Split('_').Last();
                    graphicInt = GraphicDatabase.Get(typeof(Graphic_Single), path, data.data.shaderType.Shader,
                        data.data.drawSize, data.data.color, data.data.colorTwo);
                }
                else if (data.data != null)
                {
                    graphicInt = data.data.Graphic;
                }
            }

            if (color != graphicInt.Color)
                graphicInt = graphicInt.GetColoredVersion(graphicInt.Shader, color, data.data.colorTwo);
            return graphicInt;
        }
    }

    public void Tick()
    {
        if (unused) return;
        if (data.rotationSpeed > 0)
            exactRotation += data.rotationSpeed * 0.0166666675f;
        if (ticksToBlink > 0 && blinkDuration == 0)
        {
            ticksToBlink--;
        }
        else
        {
            if (blinkDuration > 0)
                blinkDuration--;
            else
                ResetBlink();
        }
    }

    private void ResetBlink()
    {
        ticksToBlink = data.blinkInterval;
        blinkDuration = data.blinkDuration;
    }


    /*private Material ProcessMaterial(ref Material mat)
    {
        Color color = parent.ColorOverride(index);
        color = color == Color.white ? data.data.color : color;
        if (data.mode == FXMode.Blink)
        {
            color.a = 0f;
            if (blinkDuration > 0)
            {
                color.a = 1f;
            }
        }
        if (data.mode == FXMode.Pulse)
        {
            var pulse = data.pulse;
            var tick = Find.TickManager.TicksGame;
            var opaVal = TRUtils.Cosine2(pulse.opacityRange.min, pulse.opacityRange.max, pulse.opacityDuration,
                parent.tickOffset + pulse.opacityOffset, tick);
            var sizeVal = TRUtils.Cosine2(pulse.sizeRange.min, pulse.sizeRange.max, pulse.sizeDuration,
                parent.tickOffset + pulse.sizeOffset, tick);
            if (pulse.mode == PulseMode.Opacity)
                color.a = opaVal;
            else if (pulse.mode == PulseMode.Size)
                graphicInt.drawSize = size * sizeVal;
            else if (pulse.mode == PulseMode.OpaSize)
            {
                color.a = opaVal;
                graphicInt.drawSize = size * sizeVal;
            }
        }
        color.a *= parent.OpacityFloat(index);

        mat.SetColor("_Color", color);
        return mat;
    }*/

    //TODO: Improve low level rendering by abstracting from Graphic, applying changes directly to the rendering call
    //TODO: Matrix transformation a'lá 
    // Matrix4x4 matrix4x = default(Matrix4x4);
    // var pos = new Vector3(DrawPos.x, graphic.altitude, DrawPos.z + 2.55f);
    // pos.z += NodNukePosZ;
    // matrix4x.SetTRS(pos, Quaternion.Euler(Vector3.up), new Vector3(2f, 1f, 6f));

    public void Draw(Vector3 drawPos, Rot4 rot, float? rotation, Action<FXGraphic> action, int index)
    {
        if (action != null)
        {
            action.Invoke(this);
            return;
        }

        drawInfo = new GraphicDrawInfo(Graphic, drawPos, rot, ((FXThingDef)parent.parent.def).extraData,
            parent.parent.def);
        var newDrawPos = drawInfo.drawPos + data.drawOffset;
        drawMat = drawInfo.drawMat;

        drawColor = data.data.color;
        drawColor.a = parent.OpacityFloat(index);
        if (parent.ColorOverride(index) != Color.white)
            drawColor *= parent.ColorOverride(index);

        drawMat.SetTextureOffset("_MainTex", parent.TextureOffset);
        drawMat.SetTextureScale("_MainTex", parent.TextureScale);
        var drawSize = Vector2.one;
        switch (data.mode)
        {
            case FXMode.Dynamic:
                break;
            case FXMode.Mover:
                ShaderMaterial.SetTexture("_MainTex", drawMat.mainTexture);
                ShaderMaterial.SetTexture("_MaskTex", ContentFinder<Texture2D>.Get(Graphic.path + "_s"));
                drawMat = ShaderMaterial;
                var offset = new Vector2(0,
                    TRUtils.Cosine(data.startOffset, data.endOffset, data.MoverSpeed, Find.TickManager.TicksGame));
                drawMat.mainTextureOffset = offset;
                break;
            case FXMode.Blink:
                drawColor.a = 0;
                if (blinkDuration > 0)
                    drawColor.a = 1;
                break;
            case FXMode.Pulse:
                var pulse = data.pulse;
                var tick = Find.TickManager.TicksGame;
                var opaVal = TRUtils.OscillateBetween(pulse.opacityRange.min, pulse.opacityRange.max,
                    pulse.opacityDuration, tick + parent.tickOffset);
                var sizeVal = TRUtils.OscillateBetween(pulse.sizeRange.min, pulse.sizeRange.max, pulse.sizeDuration,
                    tick + parent.tickOffset);
                if (pulse.opacityRange != FloatRange.Zero)
                    drawColor.a = opaVal;
                if (pulse.sizeRange != FloatRange.Zero)
                    drawSize = drawInfo.drawSize * sizeVal;
                break;
            default:
                return;
        }

        materialProperties.SetColor(ShaderPropertyIDs.Color, drawColor);
        matrix.SetTRS(new Vector3(newDrawPos.x, altitude, newDrawPos.z),
            (exactRotation + (rotation ?? drawInfo.rotation)).ToQuat(), new Vector3(drawSize.x, 1f, drawSize.y));
        Graphics.DrawMesh(drawInfo.drawMesh, matrix, drawMat, 0, null, 0, materialProperties);
    }

    public void Print(SectionLayer layer, Vector3 drawPos, Rot4 rot, float? rotation, Thing parent)
    {
        var info = new GraphicDrawInfo(Graphic, drawPos, rot, ((FXThingDef)parent.def).extraData, parent.def);
        var newDrawPos = info.drawPos + data.drawOffset;
        Printer_Plane.PrintPlane(layer, new Vector3(newDrawPos.x, altitude, newDrawPos.z), info.drawSize, info.drawMat,
            rotation ?? info.rotation, info.flipUV);
    }
}