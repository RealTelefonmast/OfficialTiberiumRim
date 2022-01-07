using System;
using System.Linq;
using UnityEngine;
using Verse;

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

        //Unsaved data
        Vector2 drawSize = Vector2.one;
        private Material drawMat;
        private Mesh drawMesh;
        private Color drawColor;
        private float exactRotation;
        private bool flipUV;

        private float opacityFloat;
        private float sizeFloat;
        private readonly MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();

        private float AnimationSpeed => parent.AnimationSpeed(index) ?? 1;

        public MaterialPropertyBlock PropertyBlock => materialProperties;
        public float Rotation => exactRotation;

        public FXGraphic(CompFX parent, FXGraphicData data, int index)
        {
            Log.Message($"Adding Layer {index}: {data.data?.texPath} ({data.mode})");
            this.parent = parent;
            this.data = data;
            this.index = index;
            if (data.skip)
            {
                unused = true;
                return;
            }

            exactRotation = data.startRotation;
            if (data.directAltitudeOffset.HasValue)
                altitude = parent.parent.def.altitudeLayer.AltitudeFor() + data.directAltitudeOffset.Value;
            else if (data.altitude.HasValue)
                altitude = data.altitude.Value.AltitudeFor();
            else
                altitude = parent.parent.def.altitudeLayer.AltitudeFor() + (0.125f * (index + 1));
            altitude += data.extraAltitude;
            //ShaderMaterial = new Material(TiberiumContent.AlphaShaderMaterial);
        }

        public void Tick()
        {
            if (unused) return;
            if (data.rotationSpeed != 0)
                exactRotation += (AnimationSpeed * (data.rotationSpeed * 0.0166666675f));
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
                        graphicInt = GraphicDatabase.Get(typeof(Graphic_Single), path, data.data.shaderType.Shader, data.data.drawSize, data.data.color, data.data.colorTwo);
                    }
                    else if (data.data != null)
                    {
                        graphicInt = data.data.Graphic;
                    }

                    if (!data.textureParams.NullOrEmpty())
                    {
                        foreach (var param in data.textureParams)
                        {
                            param.ApplyOn(graphicInt);
                        }
                    }
                }
                return graphicInt;
            }
        }

        internal static void GetDrawInfo(Graphic g, ref Vector3 inoutPos, Rot4 rot, ExtendedGraphicData exData, ThingDef def, out Vector2 drawSize, out Material drawMat, out Mesh drawMesh, out float extraRotation, out bool flipUV)
        {
            drawMat = g.MatAt(rot);

            //DrawPos
            if ((exData?.alignToBottom ?? false) && def != null)
            {
                //Align to bottom
                float height = g.drawSize.y;
                float selectHeight = def.size.z;
                float diff = height - selectHeight;
                inoutPos.z += diff / 2;
            }
            inoutPos += exData?.drawOffset ?? Vector3.zero;
            //DrawSize
            drawSize = g.drawSize;
            bool drawRotated = exData?.drawRotatedOverride ?? g.ShouldDrawRotated;
            if (drawRotated)
            {
                flipUV = false;
            }
            else
            {
                if (rot.IsHorizontal && (exData?.rotateDrawSize ?? true))
                {
                    drawSize = drawSize.Rotated();
                }
                flipUV = /*!g.ShouldDrawRotated &&*/ ((rot == Rot4.West && g.WestFlipped) || (rot == Rot4.East && g.EastFlipped));
            }
            drawMesh = flipUV ? MeshPool.GridPlaneFlip(drawSize) : MeshPool.GridPlane(drawSize);

            //Set rotation
            if (!drawRotated)
            {
                extraRotation = 0;
                return;
            }
            float num = rot.AsAngle;
            num += g.DrawRotatedExtraAngleOffset;
            if ((rot == Rot4.West && g.WestFlipped) || (rot == Rot4.East && g.EastFlipped))
            {
                num += 180f;
            }
            extraRotation = num;
        }

        public void Draw(Vector3 drawPos, Rot4 rot, float? rotation, Action<FXGraphic> action, int index)
        {
            if(action != null)
            {
                action.Invoke(this);
                return;
            }
            GetDrawInfo(Graphic, ref drawPos, rot, ((FXThingDef)parent.parent.def).extraData, parent.parent.def, out drawSize, out drawMat, out drawMesh, out float extraRotation, out flipUV);
            var newDrawPos = drawPos + data.drawOffset;

            drawColor = data.data.color;
            drawColor.a = parent.OpacityFloat(index);
            if (parent.ColorOverride(index) != Color.white)
                drawColor *= parent.ColorOverride(index);

            drawMat.SetTextureOffset("_MainTex", parent.TextureOffset);
            drawMat.SetTextureScale("_MainTex", parent.TextureScale);

            switch (data.mode)
            {
                case FXMode.Dynamic:
                    break;
                case FXMode.Mover:
                    //TODO: Fixup the hide shader
                    ShaderMaterial.SetTexture("_MainTex", drawMat.mainTexture);
                    ShaderMaterial.SetTexture("_MaskTex", ContentFinder<Texture2D>.Get(Graphic.path + "_s"));
                    drawMat = ShaderMaterial;
                    Vector2 offset = new Vector2(0, TRUtils.Cosine(data.startOffset, data.endOffset, data.MoverSpeed, Find.TickManager.TicksGame));
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
                    var opaVal = TRUtils.OscillateBetween(pulse.opacityRange.min, pulse.opacityRange.max, pulse.opacityDuration, tick + parent.tickOffset);
                    var sizeVal = TRUtils.OscillateBetween(pulse.sizeRange.min, pulse.sizeRange.max, pulse.sizeDuration, tick + parent.tickOffset);
                    if(pulse.opacityRange != FloatRange.Zero)
                        drawColor.a = opaVal;
                    if (pulse.sizeRange != FloatRange.Zero)
                        drawSize *= sizeVal;
                    break;
                default:
                    return;
            }
            materialProperties.SetColor(ShaderPropertyIDs.Color, drawColor);

            var rotationQuat = (exactRotation + extraRotation + (rotation ?? 0)).ToQuat();

            if (data.PivotOffset != null)
            {
                var pivotPoint = newDrawPos + data.PivotOffset.Value;
                Vector3 relativePos = rotationQuat * (newDrawPos - pivotPoint);
                newDrawPos = pivotPoint + relativePos;
            }

            Graphics.DrawMesh(drawMesh, new Vector3(newDrawPos.x, altitude, newDrawPos.z), rotationQuat, drawMat, 0, null, 0, materialProperties);
        }

        public void Print(SectionLayer layer, Vector3 drawPos, Rot4 rot, float? rotation, Thing parent)
        {
            //var info = new GraphicDrawInfo(Graphic, drawPos, rot, ((FXThingDef)parent.def).extraData, parent.def);
            GetDrawInfo(Graphic, ref drawPos, rot, ((FXThingDef)parent.def).extraData, parent.def, out drawSize, out drawMat, out drawMesh, out float extraRotation, out flipUV);
            drawPos += data.drawOffset;
            Printer_Plane.PrintPlane(layer, new Vector3(drawPos.x, altitude, drawPos.z), drawSize, drawMat, (rotation ?? exactRotation) + extraRotation, flipUV);
        }
    }
}
