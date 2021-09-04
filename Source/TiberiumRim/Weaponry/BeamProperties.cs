using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class BeamProperties
    {
        public DamageDef damageDef;
        public int damageBase = 100;
        public int damageTicks = 10;

        public float stoppingPower;
        public float staggerTime = 95.TicksToSeconds();

        public float armorPenetration;

        //
        public ExplosionProperties impactExplosion;
        public EffecterDef impactEffecter;
        public FilthSpewerProperties impactFilth;

        //Graphical
        public string beamTexturePath;
        //public FloatRange scratchRange = new FloatRange(3, 4);

        public float fadeInTime = 0.25f;
        public float solidTime = 0.25f;
        public float fadeOutTime = 0.85f;

        public BeamGlow glow;
    }
}
