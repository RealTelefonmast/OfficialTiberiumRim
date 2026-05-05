using UnityEngine;
using Verse;

namespace TeleCore.Rendering;

public class Mote_Beam : TRMote
{
    //private Vector3 finalEnd;
    private Material drawMat;
    private Vector3 end;
    private Vector3 start;

    public override void ExposeData()
    {
        base.ExposeData();
    }

    public void SetConnections(Vector3 start, Vector3 end, Material mat, Color color)
    {
        this.start = start;
        this.end = end;
        //this.finalEnd = finalEnd;
        //shouldMove = puller != finalEnd;
        drawMat = mat;
        instanceColor = color;
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
        Log.Message("Removing mote with " + AgeSecs);
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        if (drawMat == null) return;

        var alpha = Alpha;
        /*if (shouldMove && AgeSecs >= def.mote.fadeInTime)
            end2 = Vector3.Lerp(puller, finalEnd, alpha);
        */
        var diff = end - start;
        if (alpha <= 0f) return;
        var color = instanceColor;
        color.a *= alpha;
        if (color != drawMat.color)
            drawMat = MaterialPool.MatFrom((Texture2D)drawMat.mainTexture, ShaderDatabase.MoteGlow, color);
        var z = diff.MagnitudeHorizontal();
        var pos = (start + end) / 2f;
        pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        var scale = new Vector3(1f, 1f, z);
        var quat = Quaternion.LookRotation(diff);
        Matrix4x4 matrix = default;
        matrix.SetTRS(pos, quat, scale);
        Graphics.DrawMesh(MeshPool.plane10, matrix, drawMat, 0);
    }
}