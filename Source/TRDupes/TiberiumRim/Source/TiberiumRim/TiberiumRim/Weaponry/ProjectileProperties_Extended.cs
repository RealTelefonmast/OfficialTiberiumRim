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

    public class LaserProperties
    {
        public string beamPath;
        public DamageDef damageDef;
        public int damageBase = 100;
        public int damageTicks = 10;

        //public FloatRange scratchRange = new FloatRange(3, 4);
        public LaserSourceGlow glow;
        public List<ThingDef> impactMotes;
    }

    public class LaserSourceGlow
    {
        public ThingDef glowMote;
        public float scale = 1;
        public float rotation = 1;
        public float rotationRate = 1;
    }
}
