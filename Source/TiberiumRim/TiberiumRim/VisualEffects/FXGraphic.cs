using System;
using System.Collections.Generic;
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

        private Material ShaderMaterial;

        public FXGraphic(CompFX parent, FXGraphicData data, int index)
        {
            Log.Message(index + " '" + data.data.texPath + "'");
            this.parent = parent;
            this.data = data;
            this.index = index;
            this.altitude = data.altitude.HasValue ? data.altitude.Value.AltitudeFor() : (parent.parent.def.altitudeLayer.AltitudeFor() + (0.125f * (index + 1)));
            ShaderMaterial = new Material(TiberiumContent.AlphaShaderMaterial);
        }

        public void Tick()
        {
            if (ticksToBlink > 0 && blinkDuration == 0)
            {
                ticksToBlink--;
            }
            else
            {
                if (blinkDuration > 0)
                {
                    blinkDuration--;
                }
                else
                {
                    ResetBlink();
                }
            }
        }

        private void ResetBlink()
        {
            ticksToBlink = data.blinkInterval;
            blinkDuration = data.blinkDuration;
        }

        public Graphic Graphic
        {
            get
            {
                Color color = parent.ColorOverride(index);              
                color = color == Color.white ? data.data.color : color;
                color.a = parent.OpacityFloat(index);
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
                    color.a = TRUtils.Cosine2(data.pulseValue.min, data.pulseValue.max, data.pulseDuration, parent.tickOffset, Find.TickManager.TicksGame);
                }       
                if(color != graphicInt.Color)
                {
                    graphicInt = graphicInt.GetColoredVersion(graphicInt.Shader, color, data.data.colorTwo);
                }
                return graphicInt;
            }
        }

        public void Draw(Vector3 drawPos, Rot4 rot, float? rotation, int index)
        {
            GraphicDrawInfo info = new GraphicDrawInfo(Graphic, parent.parent, parent.parent.def, drawPos, rot);
            Material mat = info.drawMat;
            int renderqueue = mat.renderQueue;
            if (data.mode == FXMode.Mover)
            {
                ShaderMaterial.SetTexture("_MainTex", mat.mainTexture);
                ShaderMaterial.SetTexture("_MaskTex", ContentFinder<Texture2D>.Get(Graphic.path + "_s"));
                mat = ShaderMaterial;
                mat.renderQueue = renderqueue;
                Vector2 offset = new Vector2(0, TRUtils.Cosine(data.startOffset, data.endOffset, data.MoverSpeed, Find.TickManager.TicksGame));
                mat.mainTextureOffset = offset;               
            }
            Graphics.DrawMesh(info.drawMesh, new Vector3(info.drawPos.x, altitude, info.drawPos.z), rotation?.ToQuat() ?? info.rotation.ToQuat(), mat, 0);
        }

        public void Print(SectionLayer layer, Vector3 drawPos, Rot4 rot, float? rotation, Thing parent)
        {
            var info = new GraphicDrawInfo(Graphic, parent, parent.def, drawPos, rot);
            Printer_Plane.PrintPlane(layer, new Vector3(info.drawPos.x, altitude, info.drawPos.z), info.drawSize, info.drawMat, rotation ?? info.rotation, info.flipUV);
        }
    }
}
