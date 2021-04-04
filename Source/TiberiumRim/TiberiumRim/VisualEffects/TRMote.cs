using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TRMote : MoteThrown
    {
        public float? fadeInTimeOverride;
        public float? fadeOutTimeOverride;

        private float LifeSpan => FadeInTime + SolidTime + FadeOutTime;
        protected override bool EndOfLife => AgeSecs >= LifeSpan;

        private float FadeInTime => fadeInTimeOverride ?? def.mote.fadeInTime;

        private float FadeOutTime => fadeOutTimeOverride ?? def.mote.fadeOutTime;

        public override float Alpha
        {
            get
            {
                float ageSecs = this.AgeSecs;
                if (ageSecs <= FadeInTime)
                {
                    if (FadeInTime > 0f)
                    {
                        return ageSecs / FadeInTime;
                    }
                    return 1f;
                }
                else
                {
                    if (ageSecs <= FadeInTime + SolidTime)
                    {
                        return 1f;
                    }
                    if (FadeOutTime > 0f)
                    {
                        return 1f - Mathf.InverseLerp(FadeInTime + SolidTime, LifeSpan, ageSecs);
                    }
                    return 1f;
                }
            }
        }
    }
}
