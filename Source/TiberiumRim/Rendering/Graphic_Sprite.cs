using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Graphic_Sprite : Graphic
    {
        private static Dictionary<Thing, int> indices = new Dictionary<Thing, int>();
        protected Graphic[] subGraphics;

        public int Count => subGraphics.Length;

        public int RemainingFor(Thing thing)
        {
            return subGraphics.Length - indices[thing];
        }

        public override void Init(GraphicRequest req)
        {
            this.data = req.graphicData;
            if (req.path.NullOrEmpty())
            {
                throw new ArgumentNullException("folderPath");
            }
            if (req.shader == null)
            {
                throw new ArgumentNullException("shader");
            }
            this.path = req.path;
            this.color = req.color;
            this.colorTwo = req.colorTwo;
            this.drawSize = req.drawSize;
            List<Texture2D> textures = SpritesFrom(TRUtils.GetTextureDirectory() + req.path);
            this.subGraphics = new Graphic[textures.Count];

            for (int i = 0; i < textures.Count; i++)
            {
                Graphic_SpritePart spriteGraphic = new Graphic_SpritePart();
                spriteGraphic.Init(req, textures[i]);
                this.subGraphics[i] = spriteGraphic; //GraphicDatabase.Get(typeof(Graphic_Single), path, req.shader, this.drawSize, this.color, this.colorTwo, null, req.shaderParameters);
            }
        }

        private List<Texture2D> SpritesFrom(string path)
        {
            List<Texture2D> textures = new List<Texture2D>();
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            texture2D.LoadImage(File.ReadAllBytes(path + ".png"));
            for (int y = 8 - 1; y >= 2; y--)
            {
                for (int x = 0; x < 8; x++)
                {
                    Texture2D newTex = new Texture2D(64, 64, TextureFormat.ARGB32, false);
                    newTex.SetPixels(texture2D.GetPixels(x * 64, y * 64, 64, 64));
                    newTex.Apply(true, true);
                    textures.Add(newTex);
                }
            }
            return textures;
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            CurrentGraphic(thing).DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public void AddIndex(Thing thing)
        {
            if (!indices.ContainsKey(thing))
                indices.Add(thing, 0);
        }

        public void RemoveIndex(Thing thing)
        {
            indices.Remove(thing);
        }

        public int GetIndex(Thing thing)
        {
            if (indices.TryGetValue(thing, out int i))
                return i;
            return i;
        }

        public Graphic CurrentGraphic(Thing thing)
        {
            return subGraphics[GetIndex(thing)];
        }

        public void Next(Thing thing)
        {
            if (GetIndex(thing) < Count - 1)
                indices[thing]++;
        }
    }

    public class Graphic_SpritePart : Graphic
    {
        private Material mat;

        public void Init(GraphicRequest req, Texture2D tex)
        {
            this.data = req.graphicData;
            this.path = req.path;
            this.color = req.color;
            this.colorTwo = req.colorTwo;
            this.drawSize = req.drawSize;
            this.mat = new Material(req.shader)
            {
                name = req.shader.name + "_" + tex.name,
                mainTexture = tex,
                color = req.color
            };
        }

        public override void Init(GraphicRequest req)
        {
            base.Init(req);
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return this;
        }

        public override Material MatSingleFor(Thing thing)
        {
            return this.mat;
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return this.mat;
        }

        public override Mesh MeshAt(Rot4 rot)
        {
            return base.MeshAt(rot);
        }

        public override Material MatSingle => this.mat;

        public override Material MatEast => this.mat;

        public override Material MatNorth => this.mat;

        public override Material MatSouth => this.mat;

        public override Material MatWest => this.mat;
    }
}
