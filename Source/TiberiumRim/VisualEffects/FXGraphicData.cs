using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AnimationStep
    {
        public int timeStep;
        public float timeStepSeconds;
        public int? speed;              //When null use oldStep
        public Vector3 position;

    }

    public class FXGraphicData
    {
        public GraphicData data;
        public PulseProperties pulse = new PulseProperties();
        public float? directAltitudeOffset = null;
        public float extraAltitude = 0;
        public AltitudeLayer? altitude = null;
        public FXMode mode = FXMode.Static;
        public bool needsPower = false;
        public bool skip = false;
        public int rotationSpeed;
        public int startOffset = 0;
        public int endOffset = 5;
        public int moveSpeed = 1;
        public int blinkInterval = 250;
        public int blinkDuration = 20;
        public Vector3 drawOffset = Vector3.zero;
        public Vector3? pivotOffset = null;
        public Vector3? pivotPixelOffset = null;

        public Vector3? PivotOffset
        {
            get
            {
                if (pivotOffset != null) return pivotOffset;
                if (pivotPixelOffset != null)
                {
                    var pixelOffset = pivotPixelOffset.Value;
                    var tex = data.Graphic.MatSingle.mainTexture;

                    float width = (pixelOffset.x / tex.width) * data.drawSize.x;
                    float height = (pixelOffset.z / tex.height) * data.drawSize.y;

                    pivotOffset = new Vector3(width, 0, height);
                }

                return pivotOffset;
            }
        }

        public float MoverSpeed => Mathf.Lerp(0, (endOffset - startOffset), moveSpeed);

        //private Graphic graphicInt;
        public Graphic Graphic => data.Graphic; //GraphicDatabase.Get(data.graphicClass, data.texPath, data.shaderType.Shader, data.drawSize, data.color, data.colorTwo);
    }
}
