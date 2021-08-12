using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class RadiationFallOffDef : Def
    {
        public List<ThingProbability> materialFactors;

        public float FactorFor(ThingDef material, float baseValue = 0.2f)
        {
            return materialFactors.Find(t => t.thing == material)?.probability ?? baseValue;
        }
    }
}
