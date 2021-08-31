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


        //Graphical
        public string beamPath;
        //public FloatRange scratchRange = new FloatRange(3, 4);

        public float fadeInTime = 0.25f;
        public float solidTime = 0.25f;
        public float fadeOutTime = 0.85f;

        public BeamGlow glow;
        public EffecterDef hitEffecter;
    }
}
