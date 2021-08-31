using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class ExplosionProperties
    {
        public float intensity;

        //Default
        //Explosion Props
        public float explosionRadius;
        public int? damageAmountOverride;
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
            GenExplosion.DoExplosion(center, map, explosionRadius * intensity, damageDef, instigator, damageAmountOverride ?? -1, -1f, explosionSound, null,
                null, null, postSpawnDef, postSpawnChance, postSpawnCount, 
                false, preSpawnDef, preSpawnChance, preSpawnCount, fireChance, 
                useDamageFalloff, null, null);

            explosionEffect.Spawn(center, map);
        }

    }
}
