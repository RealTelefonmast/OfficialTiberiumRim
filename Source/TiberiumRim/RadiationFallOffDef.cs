using System.Collections.Generic;
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
