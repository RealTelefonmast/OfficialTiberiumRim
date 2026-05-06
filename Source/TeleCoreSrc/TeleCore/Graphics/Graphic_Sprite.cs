using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore;

public class Graphic_Sprite : Graphic_NumberedCollection
{
    private static readonly Dictionary<Thing, int> indices = new();
    protected Graphic[] subGraphics;

    public int Count => subGraphics.Length;

    public int RemainingFor(Thing thing)
    {
        return subGraphics.Length - indices[thing];
    }

    public override void Init(GraphicRequest req)
    {
        data = req.graphicData;
        if (req.path.NullOrEmpty()) throw new ArgumentNullException("folderPath");
        if (req.shader == null) throw new ArgumentNullException("shader");
        path = req.path;
        color = req.color;
        colorTwo = req.colorTwo;
        drawSize = req.drawSize;
        var textures = SpritesFrom(TRUtils.GetTextureDirectory() + req.path);
        subGraphics = new Graphic[textures.Count];

        for (var i = 0; i < textures.Count; i++)
        {
            var spriteGraphic = new Graphic_SpritePart();
            spriteGraphic.Init(req, textures[i]);
            subGraphics[i] =
                spriteGraphic; //GraphicDatabase.Get(typeof(Graphic_Single), path, req.shader, this.drawSize, this.color, this.colorTwo, null, req.shaderParameters);
        }
    }

    private List<Texture2D> SpritesFrom(string path)
    {
        var textures = new List<Texture2D>();
        var texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texture2D.LoadImage(File.ReadAllBytes(path + ".png"));
        for (var y = 8 - 1; y >= 2; y--)
        for (var x = 0; x < 8; x++)
        {
            var newTex = new Texture2D(64, 64, TextureFormat.ARGB32, false);
            newTex.SetPixels(texture2D.GetPixels(x * 64, y * 64, 64, 64));
            newTex.Apply(true, true);
            textures.Add(newTex);
        }

        return textures;
    }

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        /*
        var extendedData = (thingDef as TRThingDef)?.extraData;
        if (extendedData != null && extendedData.repeatSprite)
        {
            int tick = extendedData.spriteTicks;
            if (thing.IsHashIntervalTick(tick))
            {
                index++;
                if (index > subGraphics.Length)
                    index = 0;
            }
        }
        */
        subGraphics[GetIndex(thing)].DrawWorker(loc, rot, thingDef, thing, extraRotation);
        //CurrentGraphic(thing).DrawWorker(loc, rot, thingDef, thing, extraRotation);
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
        if (indices.TryGetValue(thing, out var i))
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

    public override Material MatSingle => mat;

    public override Material MatEast => mat;

    public override Material MatNorth => mat;

    public override Material MatSouth => mat;

    public override Material MatWest => mat;

    public void Init(GraphicRequest req, Texture2D tex)
    {
        data = req.graphicData;
        path = req.path;
        color = req.color;
        colorTwo = req.colorTwo;
        drawSize = req.drawSize;
        mat = new Material(req.shader)
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
        return mat;
    }

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        return mat;
    }

    public override Mesh MeshAt(Rot4 rot)
    {
        return base.MeshAt(rot);
    }
    public void Notify_Remove(Thing thing)
    {
        indices.Remove(thing);
    }
}