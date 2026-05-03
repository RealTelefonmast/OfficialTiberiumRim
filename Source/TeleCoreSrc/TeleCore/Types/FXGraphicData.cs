using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class FXGraphicData
{
    public AltitudeLayer? altitude = null;
    public int blinkDuration = 20;
    public int blinkInterval = 250;
    public GraphicData data;
    public float? directAltitudeOffset = null;
    public Vector3 drawOffset = Vector3.zero;
    public int endOffset = 5;
    public float extraAltitude = 0;
    public Vector3 maxOffset;
    public FXMode mode = FXMode.Static;
    public int moveSpeed = 1;
    public bool needsPower = false;
    public PulseProperties pulse = new();
    public int rotationSpeed;
    public bool skip = false;
    public int startOffset = 0;

    public float MoverSpeed => Mathf.Lerp(0, endOffset - startOffset, moveSpeed);

    //private Graphic graphicInt;
    public Graphic Graphic =>
        data.Graphic; //GraphicDatabase.Get(data.graphicClass, data.texPath, data.shaderType.Shader, data.drawSize, data.color, data.colorTwo);
}