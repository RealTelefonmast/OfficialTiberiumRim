using RimWorld;
using Verse;

namespace TeleCore.Unsorted;

public class ExplosionProperties : Editable
{
    public DamageDef damageDef;
    public int? damageOverride;
    public EffecterDef explosionEffect;

    //Default
    //Explosion Props
    public float explosionRadius = 5f;

    public SoundDef explosionSound;

    //Misc
    public float fireChance;
    public float intensity = 1;
    public float postSpawnChance = 1f;
    public int postSpawnCount = 1;
    public ThingDef postSpawnDef;
    public float preSpawnChance = 1f;
    public int preSpawnCount = 1;

    //Spawning
    public ThingDef preSpawnDef;
    public bool useDamageFalloff;

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        damageDef = DamageDefOf.Bomb;
    }

    public void DoExplosion(IntVec3 center, Verse.Map map, Thing instigator)
    {
        GenExplosion.DoExplosion(center, map, explosionRadius * intensity, damageDef, instigator,
            damageOverride ?? -1, -1f, explosionSound, null, null, null, postSpawnDef, postSpawnChance,
            postSpawnCount, null, false, preSpawnDef, preSpawnChance, preSpawnCount, fireChance,
            useDamageFalloff);

        explosionEffect?.Spawn(center, map);
    }
}