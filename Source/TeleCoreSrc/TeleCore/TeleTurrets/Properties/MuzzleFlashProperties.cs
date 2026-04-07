using Verse;

namespace TeleCore.TeleTurrets.Properties;

public class MuzzleFlashProperties : Editable
{
    public FleckDef? fleck;
    public float scale = 1;
    public Vector3 offset = Vector3.zero;
    
    public float fadeInTime = 0f;
    public float fadeOutTime = 0f;

    public GraphicData flashGraphicData;
    private Graphic graphicInt;

    public float scale = 1;
    public float solidTime = 0.25f;

    public Graphic Graphic => graphicInt ??= flashGraphicData.Graphic;

}