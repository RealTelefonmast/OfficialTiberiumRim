using UnityEngine;
using Verse;

namespace TeleCore;

[StaticConstructorOnStartup]
public class Graphic_Particle : Graphic
{
    protected static MaterialPropertyBlock propertyBlock = new();
    public new ParticleGraphicData data;
    protected Material mat;

    public override Material MatSingle => mat;

    public override Material MatWest => mat;

    public override Material MatSouth => mat;

    public override Material MatEast => mat;

    public override Material MatNorth => mat;

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        DrawParticle(loc, null, 0);
    }

    public void DrawParticle(Vector3 loc, Particle particle, int layer)
    {
        var alpha = particle.Alpha;
        if (alpha <= 0) return;
        var color = Color * particle.Color;
        var scale = new Vector3(particle.exactScale, 0f, particle.exactScale);
        scale.x *= data.drawSize.x;
        scale.z *= data.drawSize.y;
        var matrix = default(Matrix4x4);
        matrix.SetTRS(particle.exactPos, Quaternion.AngleAxis(particle.exactRotation, Vector3.up), scale);
        var material = MatSingle;

        propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, MatSingle, layer, null, 0, propertyBlock);
    }

    public override void Init(GraphicRequest req)
    {
        data = req.graphicData as ParticleGraphicData;
        path = req.path;
        color = req.color;
        colorTwo = req.colorTwo;
        drawSize = req.drawSize;
        var req2 = default(MaterialRequest);
        req2.mainTex = ContentFinder<Texture2D>.Get(req.path);
        req2.shader = req.shader;
        req2.color = color;
        req2.colorTwo = colorTwo;
        req2.renderQueue = req.renderQueue;
        req2.shaderParameters = req.shaderParameters;
        if (req.shader.SupportsMaskTex())
            req2.maskTex = ContentFinder<Texture2D>.Get(req.path + Graphic_Single.MaskSuffix, false);
        mat = MaterialPool.MatFrom(req2);
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return GraphicDatabase.Get<Graphic_Particle>(path, newShader, drawSize, newColor, newColorTwo, data);
    }
}