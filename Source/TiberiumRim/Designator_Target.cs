using TeleCore.Utility;
using TR.ThingData;
using TR.Util;
using UnityEngine;
using Verse;

namespace TR;

public abstract class Designator_Target : Designator_Extended
{
    protected FloatRange opacity = new(0.5f, 1f);
    protected float size;
    public ISuperWeapon superWeapon;
    protected Material targeterMat;
    private Material tempMaterial;

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        return loc.InBounds(Map) && !loc.Fogged(Map);
    }

    public override void SelectedUpdate()
    {
        if (targeterMat == null)
            return;

        tempMaterial = new Material(targeterMat);
        var designateCheck = CanDesignateCell(UI.MouseCell()).Accepted;
        var color = !designateCheck ? Color.red : targeterMat.color;
        if (opacity.min != opacity.max)
            color.a = TMath.Cosine2(opacity.min, opacity.max, 3f, 0, Time.realtimeSinceStartup * 6.28318548f);
        tempMaterial.color = color;
        TRUtils.DrawTargeter(UI.MouseCell(), tempMaterial, size);
    }
}