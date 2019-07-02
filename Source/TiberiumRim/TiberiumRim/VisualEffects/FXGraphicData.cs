using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class FXGraphicData
    {
        public GraphicData data;
        public AltitudeLayer? altitude = null;
        public FXMode mode = FXMode.Static;
        public bool needsPower = false;
        public int startOffset = 0;
        public int endOffset = 5;
        public int moveSpeed = 1;
        public int blinkInterval = 250;
        public int blinkDuration = 20;
        public int pulseDuration = 60;
        public FloatRange pulseValue = new FloatRange(0f, 1f);
        public Vector3 maxOffset;

        public float MoverSpeed => Mathf.Lerp(0, (endOffset - startOffset), moveSpeed);

        public Graphic Graphic()
        {
            return GraphicDatabase.Get(data.graphicClass, data.texPath, data.shaderType.Shader, data.drawSize, data.color, data.colorTwo);
        }
    }
}
