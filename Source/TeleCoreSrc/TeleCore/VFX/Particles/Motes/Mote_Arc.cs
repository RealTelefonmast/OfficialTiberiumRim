using UnityEngine;
using Verse;

namespace TeleCore.VFX.Particles.Motes;

public class Mote_Arc : TeleMote
{
    private Material drawMat;
    private Vector3 end;
    private Vector3 start;

    public void SetConnections(Vector3 start, Vector3 end, Material mat, Color color)
    {
        this.start = start;
        this.end = end;
        drawMat = mat;
        instanceColor = color;
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        if (drawMat == null) return;
        var alpha = Alpha;

        var diff = end - start;
        if (alpha <= 0f) return;
        var color = instanceColor;
        color.a *= alpha;
        if (color != drawMat.color)
            drawMat = MaterialPool.MatFrom((Texture2D) drawMat.mainTexture, ShaderDatabase.MoteGlow, color);
        var z = diff.MagnitudeHorizontal();
        var x = diff.MagnitudeHorizontal();
        var pos = (start + end) / 2f;
        pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        var scale = new Vector3(z / 2, 1f, z);
        var quat = Quaternion.LookRotation(diff);
        Matrix4x4 matrix = default;
        matrix.SetTRS(pos, quat, scale);
        Graphics.DrawMesh(MeshPool.plane10, matrix, drawMat, 0);
    }
}