using UnityEngine;
using Verse;

namespace TiberiumRim;

public class FXGraphicData
{
    public AltitudeLayer? altitude = null;
    public int blinkDuration = 20;
    public int blinkInterval = 250;
    public GraphicData data;
    public float? directAltitudeOffset = null;
    public int endOffset = 5;
    public Vector3 maxOffset;
    public FXMode mode = FXMode.Static;
    public int moveSpeed = 1;
    public bool needsPower = false;
    public PulseProperties pulse = new();
    public bool skip = false;
    public int startOffset = 0;

    public float MoverSpeed => Mathf.Lerp(0, endOffset - startOffset, moveSpeed);

    public Graphic Graphic()
    {
        return GraphicDatabase.Get(data.graphicClass, data.texPath, data.shaderType.Shader, data.drawSize, data.color,
            data.colorTwo);
    }
}