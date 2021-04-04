using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ProjectileProperties_Extended
    {
        public ExplosionProperties explosion;
    }

    public class ExplosionProperties
    {
        public float explosionRadius;
        public int explosionDelay;

        public ThingDef preSpawnDef;
        public ThingDef postSpawnDef;
        public float preSpawnChance  = 1f;
        public float postSpawnChance = 1f;
        public int preSpawnCount     = 1;
        public int postSpawnCount    = 1;
        public float shakeFactor     = 1f;
        public float fireChance;
        public bool explosionDamageFalloff;
        public EffecterDef explosionEffect;
        public DamageDef damageDef;

        public SoundDef soundExplode;

    }

    public class BeamProperties
    {
        public DamageDef damageDef;
        public int damageBase = 100;
        public int damageTicks = 10;


        //Graphical
        public string beamPath;
        //public FloatRange scratchRange = new FloatRange(3, 4);

        public float fadeInTime = 0.25f;
        public float solidTime = 0.25f;
        public float fadeOutTime = 0.85f;

        public BeamGlow glow;
        public EffecterDef hitEffecter;
    }

    public class BeamGlow
    {
        public ThingDef glowMote;
        public float scale = 1;
        public float rotation = 1;
        public float rotationRate = 1;
    }
}
