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
        
    }

    public class ExplosionProperties
    {
        public float explosionRadius;
        public float preSpawnChance;
        public float postSpawnChance;
        public DamageDef damageDef;
        public ThingDef postSpawnDef;
        public ThingDef preSpawnDef;
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
