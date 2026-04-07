using System.Collections.Generic;
using TR.GameParts;
using Verse;

namespace TR.Defs;

public class RadiationFallOffDef : Def
{
    public List<DefFloat<ThingDef>> materialFactors;

    public float FactorFor(ThingDef material, float baseValue = 0.2f)
    {
        return materialFactors.Find(t => t.def == material)?.value ?? baseValue;
    }
}