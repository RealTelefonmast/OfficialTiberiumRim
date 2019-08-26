using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class PulseProperties
    {
        public PulseMode mode = PulseMode.Opacity;
        public int opacityDuration = 60;
        public int sizeDuration = 60;
        public FloatRange opacityRange = new FloatRange(0f, 1f);
        public FloatRange sizeRange = new FloatRange(0.5f, 1f);
        public int opacityOffset;
        public int sizeOffset;
    }

    public enum PulseMode
    {
        Opacity,
        Size,
        OpaSize
    }
}
