using Verse;

namespace TiberiumRim
{
    public class ProjectileProperties_Extended
    {
        public ExplosionProperties impactExplosion;
        public EffecterDef impactEffecter;
        public FilthSpewerProperties impactFilth;
    }

    public class BeamGlow
    {
        public ThingDef glowMote;
        public float scale = 1;
        public float rotation = 1;
        public float rotationRate = 1;
    }
}
