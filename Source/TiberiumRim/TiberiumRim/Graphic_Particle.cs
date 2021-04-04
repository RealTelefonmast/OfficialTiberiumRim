using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class Graphic_Particle : Graphic
    {
        protected static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        public new ParticleGraphicData data;
        protected Material mat;       

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            DrawParticle(loc, null, 0);
        }

        public void DrawParticle(Vector3 loc, Particle particle, int layer)
        {
            float alpha = particle.Alpha;
            if(alpha <= 0)
            {
                return;
            }
            Color color = base.Color * particle.Color;
            Vector3 scale = new Vector3(particle.exactScale,0f, particle.exactScale);
            scale.x *= this.data.drawSize.x;
            scale.z *= this.data.drawSize.y;
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(particle.exactPos, Quaternion.AngleAxis(particle.exactRotation, Vector3.up), scale);
            Material material = MatSingle;

            propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
            Graphics.DrawMesh(MeshPool.plane10, matrix, MatSingle, layer, null, 0, propertyBlock);
        }

        public override void Init(GraphicRequest req)
        {
            data = req.graphicData as ParticleGraphicData;
            path = req.path;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;
            MaterialRequest req2 = default(MaterialRequest);
            req2.mainTex = ContentFinder<Texture2D>.Get(req.path, true);
            req2.shader = req.shader;
            req2.color = this.color;
            req2.colorTwo = this.colorTwo;
            req2.renderQueue = req.renderQueue;
            req2.shaderParameters = req.shaderParameters;
            if (req.shader.SupportsMaskTex())
            {
                req2.maskTex = ContentFinder<Texture2D>.Get(req.path + Graphic_Single.MaskSuffix, false);
            }
            mat = MaterialPool.MatFrom(req2);
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_Particle>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data);
        }

        public override Material MatSingle => this.mat;

        public override Material MatWest => this.mat;

        public override Material MatSouth => this.mat;

        public override Material MatEast => this.mat;

        public override Material MatNorth => this.mat;
    }
}
