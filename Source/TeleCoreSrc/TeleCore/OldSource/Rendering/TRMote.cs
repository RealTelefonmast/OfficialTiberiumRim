using UnityEngine;
using Verse;

namespace TeleCore.Rendering;

public class TRMote : MoteThrown
{
    public float? fadeInTimeOverride;
    public float? fadeOutTimeOverride;

    private float LifeSpan => FadeInTime + SolidTime + FadeOutTime;
    public override bool EndOfLife => AgeSecs >= LifeSpan;

    private float FadeInTime => fadeInTimeOverride ?? def.mote.fadeInTime;

    private float FadeOutTime => fadeOutTimeOverride ?? def.mote.fadeOutTime;

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
}