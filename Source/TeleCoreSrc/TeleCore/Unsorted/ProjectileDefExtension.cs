using Verse;

namespace TeleCore.Unsorted;

/// <summary>
///     If set on a projectile's def, on Impact, all of these effects will be created - if available.
/// </summary>
public class ProjectileDefExtension
{
    public EffecterDef impactEffecter;
    public ExplosionProperties impactExplosion;
    public FilthSpawnerProperties impactFilth;
}