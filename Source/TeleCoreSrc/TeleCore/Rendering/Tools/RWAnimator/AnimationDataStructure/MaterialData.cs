using RimWorld;
using TeleCore.Utility;
using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator.AnimationDataStructure;

public struct MaterialData : IExposable
{
    private Color color;
    private string texPath;
    private string shaderPath;

    [Unsaved] private WrappedTexture texture;
    [Unsaved] private Shader shader;

    public WrappedTexture Texture
    {
        get
        {
            if (texture.IsValid) return texture;
            return texture = new WrappedTexture(texPath, ContentFinder<Texture2D>.Get(texPath));
        }
    }

    public Shader Shader
    {
        get { return shader ??= ShaderDatabase.LoadShader(shaderPath); }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref color, nameof(color), forceSave: true);
        Scribe_Values.Look(ref texPath, nameof(texPath), forceSave: true);
        Scribe_Values.Look(ref shaderPath, nameof(shaderPath), forceSave: true);

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            texture = new WrappedTexture(texPath, ContentFinder<Texture2D>.Get(texPath));
            shader = ShaderDatabase.LoadShader(shaderPath);
        }
    }

    public MaterialData(WrappedTexture fromTexture)
    {
        texPath = fromTexture.Path;
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
        texPath = texture.Path;
        color = fromMat.color;
    }

    public Material GetMat()
    {
        var materialInt = new Material(Shader);
        materialInt.name = $"{texPath}";
        materialInt.mainTexture = Texture.Texture;
        materialInt.color = color;
        return materialInt;
    }
}