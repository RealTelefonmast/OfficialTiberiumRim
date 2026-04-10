using Verse;

namespace TR;

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

public class BeamProperties
{
    //Graphical
    public string beamPath;
    public int damageBase = 100;
    public DamageDef damageDef;

    public int damageTicks = 10;
    //public FloatRange scratchRange = new FloatRange(3, 4);

    public float fadeInTime = 0.25f;
    public float fadeOutTime = 0.85f;

    public BeamGlow glow;
    public EffecterDef hitEffecter;
    public float solidTime = 0.25f;
}

public class BeamGlow
{
    public ThingDef glowMote;
    public float rotation = 1;
    public float rotationRate = 1;
    public float scale = 1;
}