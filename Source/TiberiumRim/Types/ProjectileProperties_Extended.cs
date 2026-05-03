using System.Collections.Generic;
using Verse;

namespace TiberiumRim;

public class ProjectileProperties_Extended
{
    public ExplosionProperties explosion;
}

public class ExplosionProperties
{
    public DamageDef damageDef;
    public bool explosionDamageFalloff;
    public int explosionDelay;
    public EffecterDef explosionEffect;
    public float explosionRadius;
    public float fireChance;
    public float postSpawnChance = 1f;
    public int postSpawnCount = 1;
    public ThingDef postSpawnDef;
    public float preSpawnChance = 1f;
    public int preSpawnCount = 1;

    public ThingDef preSpawnDef;
    public float shakeFactor = 1f;

    public SoundDef soundExplode;
}

public class LaserProperties
{
    public string beamPath;
    public int damageBase = 100;
    public DamageDef damageDef;
    public int damageTicks = 10;

    //public FloatRange scratchRange = new FloatRange(3, 4);
    public LaserSourceGlow glow;
    public List<ThingDef> impactMotes;
}

public class LaserSourceGlow
{
    public ThingDef glowMote;
    public float rotation = 1;
    public float rotationRate = 1;
    public float scale = 1;
}