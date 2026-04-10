using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class ParticleDef : Def
    {
        public ParticleGraphicData graphicData;
        public Type particleClass = typeof(Particle);
        public bool shouldBeSaved = false;
        public bool realtime = true;
        public AltitudeLayer altitudeLayer = AltitudeLayer.MoteOverhead;
        public ParticleMovement movement = ParticleMovement.Stationary;

        public float solidTime = 1;
        public float fadeOutTime = 0.5f;
        public float fadeInTime = 0.5f;
        public float rotationSpeed = 0f;
        public float frequency = 0.5f;
        public Vector2 direction = new Vector2(0, 1f);
        public FloatRange sizeRange = new FloatRange(1f, 1f);
        public FloatRange speedRange = new FloatRange(0.1f, 1f);
        public FloatRange wiggleRange = new FloatRange(0f, 0f);
    }

    public enum ParticleMovement
    {
        Stationary,
        Path,
        Spiral
    }
}
