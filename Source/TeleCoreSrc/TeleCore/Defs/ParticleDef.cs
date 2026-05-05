using System;
using TeleCore.Graphics;
using TeleCore.Types.Entities;
using UnityEngine;
using Verse;

namespace TeleCore.Defs;

public class ParticleDef : Def
{
    public AltitudeLayer altitudeLayer = AltitudeLayer.MoteOverhead;
    public Vector2 direction = new(0, 1f);
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;
    public float frequency = 0.5f;
    public ParticleGraphicData graphicData;
    public ParticleMovement movement = ParticleMovement.Stationary;
    public Type particleClass = typeof(Particle);
    public Type particleSystemClass;
    public bool realtime = true;
    public float rotationSpeed = 0f;
    public bool shouldBeSaved = false;
    public FloatRange sizeRange = new(1f, 1f);

    public float solidTime = 1;
    public FloatRange speedRange = new(0.1f, 1f);
    public FloatRange wiggleRange = new(0f, 0f);
}

public enum ParticleMovement
{
    Stationary,
    Path,
    Spiral
}