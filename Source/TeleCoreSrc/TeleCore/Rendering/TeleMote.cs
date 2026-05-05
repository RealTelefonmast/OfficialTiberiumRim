using UnityEngine;
using Verse;

namespace TeleCore.Rendering;

public class TeleMote : MoteThrown
{
    public float? fadeInTimeOverride;
    public float? fadeOutTimeOverride;
    protected MaterialPropertyBlock materialProps;

    public override bool EndOfLife => AgeSecs >= LifeSpan;

    private float LifeSpan => FadeInTime + SolidTime + FadeOutTime;
    private float FadeInTime => fadeInTimeOverride ?? def.mote.fadeInTime;
    private float FadeOutTime => fadeOutTimeOverride ?? def.mote.fadeOutTime;

    public Material AttachedMat { get; private set; }

    public override float Alpha
    {
        get
        {
            var ageSecs = AgeSecs;
            if (ageSecs <= FadeInTime)
            {
                if (FadeInTime > 0f) return ageSecs / FadeInTime;
                return 1f;
            }

            if (ageSecs <= FadeInTime + SolidTime) return 1f;
            if (FadeOutTime > 0f) return 1f - Mathf.InverseLerp(FadeInTime + SolidTime, LifeSpan, ageSecs);
            return 1f;
        }
    }

    public void SetTimeOverrides(float? fadeIn, float? fadeOut)
    {
        fadeInTimeOverride = fadeIn;
        fadeOutTimeOverride = fadeOut;
    }

    public void AttachMaterial(Material newMat, Color color)
    {
        //TLog.Message($"Attaching mat: {newMat}");
        AttachedMat = newMat;
        instanceColor = color;
    }
}