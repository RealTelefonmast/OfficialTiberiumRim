﻿using Verse;

namespace TiberiumRim
{
    public class ExplosionProperties
    {
        public float intensity = 1;

        //Default
        //Explosion Props
        public float explosionRadius;
        public int? damageOverride;
        public DamageDef damageDef;

        //Spawning
        public ThingDef preSpawnDef;
        public ThingDef postSpawnDef;
        public float preSpawnChance = 1f;
        public float postSpawnChance = 1f;
        public int preSpawnCount = 1;
        public int postSpawnCount = 1;

        //Misc
        public float fireChance;
        public bool useDamageFalloff;
        public EffecterDef explosionEffect;

        public SoundDef explosionSound;

        public void DoExplosion(IntVec3 center, Map map, Thing instigator)
        {
            GenExplosion.DoExplosion(center, map, explosionRadius * intensity, damageDef, instigator, 
                damageOverride ?? -1, -1, explosionSound, null, null, null, postSpawnDef, postSpawnChance, postSpawnCount,
                GasType.Unused, false, preSpawnDef, preSpawnChance, preSpawnCount, fireChance,
                useDamageFalloff, null, null, null);

            explosionEffect?.Spawn(center, map);
        }

    }
}
