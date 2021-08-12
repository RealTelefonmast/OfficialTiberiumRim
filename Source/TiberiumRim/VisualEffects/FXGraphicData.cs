using UnityEngine;
using Verse;

namespace TiberiumRim
{
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

        public float MoverSpeed => Mathf.Lerp(0, (endOffset - startOffset), moveSpeed);

        //private Graphic graphicInt;
        public Graphic Graphic => data.Graphic; //GraphicDatabase.Get(data.graphicClass, data.texPath, data.shaderType.Shader, data.drawSize, data.color, data.colorTwo);
    }
}
