using UnityEngine;
using Verse;

namespace TR.Hediffs.Drawing;

public class HediffComp_Icon : HediffComp
{
    private Texture2D icon;

    public HediffCompProperties_Icon Props => (HediffCompProperties_Icon)props;

    public override TextureAndColor CompStateIcon
    {
        get
        {
            if (icon == null) icon = ContentFinder<Texture2D>.Get(Props.iconPath);
            return icon;
        }
    }
}

public class HediffCompProperties_Icon : HediffCompProperties
{
    public string iconPath = "";

    public HediffCompProperties_Icon()
    {
        compClass = typeof(HediffComp_Icon);
    }
}