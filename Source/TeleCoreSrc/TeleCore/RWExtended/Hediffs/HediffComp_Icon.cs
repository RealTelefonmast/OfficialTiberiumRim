using UnityEngine;
using Verse;

namespace TeleCore.RWExtended.Hediffs;

public class HediffComp_Icon : HediffComp
{
    private TextureAndColor icon;

    public HediffCompProperties_Icon Props => (HediffCompProperties_Icon)props;

    public override TextureAndColor CompStateIcon
    {
        get
        {
            if (!icon.HasValue) icon = new TextureAndColor(ContentFinder<Texture2D>.Get(Props.iconPath), Props.color);
            return icon;
        }
    }
}

public class HediffCompProperties_Icon : HediffCompProperties
{
    public Color color = Color.white;
    public string iconPath = null;

    public HediffCompProperties_Icon()
    {
        compClass = typeof(HediffComp_Icon);
    }
}