using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator.AnimationDataStructure;

public struct TextureData : IExposable
{
    private MaterialData materialData;
    private bool attachScript = false;
    private string layerTag = null;
    private TexCoordAnchor anchor = this.TexCoordAnchor.Center;
    private TexStretchMode stretchMode = TexStretchMode.Normal;
    private int layerIndex = 0;

    //
    private Rect texCoordsReference;

    [Unsaved] private Material matInt;

    private Texture Texture => Material.mainTexture;

    public Material Material
    {
        get
        {
            if (matInt != null) return matInt;

            //FirstTimeSetup
            matInt = materialData.GetMat();
            matInt.SetTextureOffset("_MainTex", texCoordsReference.position);
            matInt.SetTextureScale("_MainTex", texCoordsReference.size);
            return matInt;
        }
    }

    public Rect TexCoordReference => texCoordsReference;

    public TexStretchMode StretchMode
    {
        get => stretchMode;
        set => stretchMode = value;
    }

    public TexCoordAnchor TexCoordAnchor
    {
        get => anchor;
        set => anchor = value;
    }

    public bool AttachScript
    {
        get => attachScript;
        set => attachScript = value;
    }

    public string LayerTag
    {
        get => layerTag;
        set => layerTag = value;
    }

    public string StringBuffer { get; set; } = "0";

    public int LayerIndex
    {
        get => layerIndex;
        set => layerIndex = value;
    }

    //
    public void ExposeData()
    {
        /*
        if (AnimationFileSaveLoader._OLDLOAD)
        {
            Scribe_Deep.Look(ref materialData, "materialData");
            Scribe_Rect.Look(ref texCoordsReference, "texCoords");
            Scribe_Values.Look(ref attachScript, "attacheScript");
            Scribe_Values.Look(ref layerTag, "layerTag");
            Scribe_Values.Look(ref layerIndex, "layerIndex");
            Scribe_Values.Look(ref anchor, "anchor");
        }
        */

        Scribe_Deep.Look(ref materialData, nameof(materialData));
        Scribe_Rect.Look(ref texCoordsReference, nameof(texCoordsReference), new Rect(0, 0, 1, 1));
        Scribe_Values.Look(ref attachScript, nameof(attachScript));
        Scribe_Values.Look(ref layerTag, nameof(layerTag));
        Scribe_Values.Look(ref layerIndex, nameof(layerIndex));
        Scribe_Values.Look(ref anchor, nameof(anchor));
        Scribe_Values.Look(ref stretchMode, nameof(stretchMode));

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            matInt = materialData.GetMat();
            matInt.SetTextureOffset("_MainTex", texCoordsReference.position);
            matInt.SetTextureScale("_MainTex", texCoordsReference.size);
        }
    }

    public void SetTexCoordReference(Rect texCoords)
    {
        texCoordsReference = texCoords;
    }

    public TextureData(WrappedTexture texture)
    {
        materialData = new MaterialData(texture);
        matInt = materialData.GetMat();
        texCoordsReference = new Rect(0, 0, 1, 1);
    }

    public TextureData(Material material)
    {
        materialData = new MaterialData(material);
        matInt = material;
        texCoordsReference = new Rect(0, 0, 1, 1);
    }
}