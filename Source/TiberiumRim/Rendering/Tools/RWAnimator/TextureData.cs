using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public struct MaterialData : IExposable
    {
        private Color color;
        private string texPath;
        private string shaderPath;

        [Unsaved] 
        private WrappedTexture texture;
        [Unsaved]
        private Shader shader;

        public void ExposeData()
        {
            Scribe_Values.Look(ref texPath, "texPath", forceSave: true);
            Scribe_Values.Look(ref color, "color", forceSave: true); 
            Scribe_Values.Look(ref shaderPath, "shaderPath", forceSave: true);
        }

        public MaterialData(WrappedTexture fromTexture)
        {
            this.texPath = fromTexture.path;
            shader = ShaderDatabase.CutoutComplex;
            shaderPath = ShaderTypeDefOf.CutoutComplex.shaderPath;
            color = Color.white;
            texture = fromTexture;
        }

        public MaterialData(Material fromMat)
        {
            shader = fromMat.shader;
            shaderPath = fromMat.shader.Location();
            texture = new WrappedTexture(fromMat.mainTexture.Location(), fromMat.mainTexture);
            texPath = texture.path;
            color = fromMat.color;
        }

        public Material GetMat()
        {
            var materialInt = new Material(shader);
            materialInt.mainTexture = texture.texture;
            materialInt.color = color;
            return materialInt;
        }
    }

    public struct TextureData : IExposable
    {
        private MaterialData materialData;
        private KeyFrameData localData;
        private Rect? texCoords;
        private Vector2 pivotPoint;

        [Unsaved]
        private Material matInt;

        private Texture Texture => Material.mainTexture;
        public Material Material => matInt;

        public Vector2 TextureSize => new (Texture.width, Texture.height);
        public Rect? TexCoords => texCoords;

        public KeyFrameData LocalData
        {
            get => localData;
            set => localData = value;
        }

        public Vector2 PivotPoint
        {
            get => pivotPoint;
            set => pivotPoint = value;
        }

        public Vector2 TSize
        {
            set => localData.TSize = value;
        }

        public Vector2 TPosition
        {
            get => localData.TPosition;
            set => localData.TPosition = value;
        }

        public float TRotation
        {
            set => localData.TRotation = value;
        }

        public Vector2 TSizeFactor => TextureSize /2f;

        public void ExposeData()
        {
            Scribe_Deep.Look(ref materialData, "materialData");
            Scribe_Deep.Look(ref localData, "localData");
            Scribe_Values.Look(ref texCoords, "texCoords", forceSave:true);
            Scribe_Values.Look(ref pivotPoint, "pivotPoint", forceSave:true);
        }

        public TextureData(WrappedTexture texture)
        {
            materialData = new MaterialData(texture);
            matInt = materialData.GetMat();
            localData = new KeyFrameData(default, 0, default);
            texCoords = null;
            pivotPoint = Vector2.zero;
        }

        public TextureData(Material material)
        {
            materialData = new MaterialData(material);
            matInt = material;
            localData = new KeyFrameData(default, 0, default);
            texCoords = null;
            pivotPoint = Vector2.zero;
        }

        public void SetTRS(Vector2 pos, float rot, Vector2 size)
        {
            TPosition = pos;
            TRotation = rot;
            TSize = size;
        }

        public void SetTexCoords(Rect rect)
        {
            texCoords ??= rect;
        }
    }
}
