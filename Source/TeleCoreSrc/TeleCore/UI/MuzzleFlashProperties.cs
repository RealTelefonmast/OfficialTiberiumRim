using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class MuzzleFlashProperties : Editable
{
    public float fadeInTime = 0f;
    public float fadeOutTime = 0f;

    public GraphicData flashGraphicData;
    public FleckDef? fleck;
    private Graphic graphicInt;
    public Vector3 offset = Vector3.zero;
    public float scale = 1;

    public float scale = 1;
    public float solidTime = 0.25f;

    public Graphic Graphic => graphicInt ??= flashGraphicData.Graphic;
}