using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class FXGraphic
    {
        private CompFX parent;
        public Graphic graphicInt;        
        public FXGraphicData data;
        public float altitude;
        public int ticksToBlink = 0;
        public int blinkDuration = 0;
        public int index = 0;

        private bool unused;
        private Material ShaderMaterial;
        
        public FXGraphic(CompFX parent, FXGraphicData data, int index)
        {
            Log.Message(index + " '" + data.data?.texPath + "'" + " which is " + data.mode);
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
                altitude = parent.parent.def.altitudeLayer.AltitudeFor() + (0.125f * (index + 1));
            altitude += data.extraAltitude;
            ShaderMaterial = new Material(TiberiumContent.AlphaShaderMaterial);
        }

        public void Tick()
        {
            if (unused) return;

            if (ticksToBlink > 0 && blinkDuration == 0)
                ticksToBlink--;
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

        //TODO: Reduce creation of new Graphic instances, go low level unity rendering.
        public Graphic Graphic
        {
            get
            {
                if (graphicInt == null)
                {
                    if(parent.parent.Graphic is Graphic_Random random)
                    {
                        var path = this.data.data.texPath;
                        var parentName = random.SubGraphicFor(parent.parent).path.Split('/').Last();
                        var lastPart = path.Split('/').Last();
                        path += "/" + lastPart;
                        path += "_" + parentName.Split('_').Last();
                        Log.Message("Whole Path: " + path);
                        graphicInt = GraphicDatabase.Get(typeof(Graphic_Single), path, data.data.shaderType.Shader, data.data.drawSize, data.data.color, data.data.colorTwo);
                    }
                    else if (data.data != null)
                    {
                        graphicInt = data.data.Graphic;
                    }
                }
                return graphicInt;
            }
        }


        //TODO: Improve low level rendering by abstracting from Graphic, applying changes directly to the rendering call
        //TODO: Matrix transformation a'lá 
        // Matrix4x4 matrix4x = default(Matrix4x4);
        // var pos = new Vector3(DrawPos.x, graphic.altitude, DrawPos.z + 2.55f);
        // pos.z += NodNukePosZ;
        // matrix4x.SetTRS(pos, Quaternion.Euler(Vector3.up), new Vector3(2f, 1f, 6f));

        public void Draw(Vector3 drawPos, Rot4 rot, float? rotation, Action<FXGraphic> action, int index)
        {
            if(action != null)
            {
                action.Invoke(this);
                return;
            }
            GraphicDrawInfo info = new GraphicDrawInfo(Graphic, drawPos, rot, ((FXThingDef)parent.parent.def).extraData, parent.parent.def);
            Material mat = info.drawMat;

            Color color = data.data.color;
            if (parent.ColorOverride(index) != Color.white)
                color = parent.ColorOverride(index);

            mat.SetTextureOffset("_MainTex", parent.TextureOffset);
            mat.SetTextureScale("_MainTex", parent.TextureScale);

            switch (data.mode)
            {
                case FXMode.Dynamic:
                    break;
                case FXMode.Mover:
                    ShaderMaterial.SetTexture("_MainTex", mat.mainTexture);
                    ShaderMaterial.SetTexture("_MaskTex", ContentFinder<Texture2D>.Get(Graphic.path + "_s"));
                    mat = ShaderMaterial;
                    Vector2 offset = new Vector2(0, TRUtils.Cosine(data.startOffset, data.endOffset, data.MoverSpeed, Find.TickManager.TicksGame));
                    mat.mainTextureOffset = offset;
                    break;
                case FXMode.Blink:
                    color.a = 0;
                    if (blinkDuration > 0)
                        color.a = 1;
                    break;
                case FXMode.Pulse:
                    var pulse = data.pulse;
                    var tick = Find.TickManager.TicksGame;
                    var opaVal = TRUtils.Cosine2(pulse.opacityRange.min, pulse.opacityRange.max, pulse.opacityDuration,
                        parent.tickOffset + pulse.opacityOffset, tick);
                    var sizeVal = TRUtils.Cosine2(pulse.sizeRange.min, pulse.sizeRange.max, pulse.sizeDuration,
                        parent.tickOffset + pulse.sizeOffset, tick);
                    if (pulse.mode == PulseMode.Opacity)
                        color *= opaVal;
                    else if (pulse.mode == PulseMode.Size)
                        graphicInt.drawSize = info.drawSize * sizeVal;
                    else if (pulse.mode == PulseMode.OpaSize)
                    {
                        color *= opaVal;
                        graphicInt.drawSize = info.drawSize * sizeVal;
                    }
                    break;
                default:
                    return;
            }
            mat.SetColor(ShaderPropertyIDs.Color, color);

            Graphics.DrawMesh(info.drawMesh, new Vector3(info.drawPos.x, altitude, info.drawPos.z), rotation?.ToQuat() ?? info.rotation.ToQuat(), mat, 0);
        }

        public void Print(SectionLayer layer, Vector3 drawPos, Rot4 rot, float? rotation, Thing parent)
        {
            var info = new GraphicDrawInfo(Graphic, drawPos, rot, ((FXThingDef)parent.def).extraData, parent.def);
            Printer_Plane.PrintPlane(layer, new Vector3(info.drawPos.x, altitude, info.drawPos.z), info.drawSize, info.drawMat, rotation ?? info.rotation, info.flipUV);
        }
    }
}
